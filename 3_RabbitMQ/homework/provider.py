import pika
from message_receiver import MessageReceiver
import threading
import sys

# config
all_services = ["people", "cargo", "satellites"]

if len(sys.argv) > 1:
    provider = int(sys.argv[1])
else:
    provider = 1
mailbox_id = "provider{}".format(provider)
services = [all_services[i % len(all_services)] for i in [provider, provider + 1]]
print("Provider {} starting as {} with services".format(provider, mailbox_id), services)

names_bindings = {
    'people': "#.ppl.#",
    'satellites': "#.sat.#",
    'cargo': "#.cgo.#"
}

exchange_name = 'business'
admin_ex_name = 'admin'

# administrative message queue:
receiver = MessageReceiver(mailbox_id)
receiver.bind_to_exchange(admin_ex_name,
                          ['#.' + mailbox_id + '.#',
                           '#.providers.#',
                           '#.all.#'])
admin_thread = threading.Thread(target=receiver.start_receiving)
admin_thread.start()

connection = pika.BlockingConnection(
    pika.ConnectionParameters(host='localhost'))
channel = connection.channel()

channel.exchange_declare(
    exchange=exchange_name,
    exchange_type='topic')


def callback(ch, method, properties, body):
    print("Provider {} received {}".format(provider, body))
    order_data = body.decode('utf-8').split()
    if len(order_data) == 7:
        ret_key = 'agency' + order_data[2]
        body = 'Provider {} has executed task {} of type {}'. \
            format(provider, order_data[4], order_data[6])
        ch.basic_publish(exchange=exchange_name,
                         routing_key=ret_key,
                         body=body)


# starting queues
for queue_name in services:
    channel.queue_declare(queue=queue_name)
    channel.queue_bind(
        exchange=exchange_name,
        queue=queue_name,
        routing_key=names_bindings[queue_name])
    channel.basic_consume(
        queue=queue_name,
        on_message_callback=callback,
        auto_ack=True)
channel.start_consuming()
