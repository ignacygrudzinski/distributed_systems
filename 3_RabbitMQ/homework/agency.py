#!/usr/bin/env python
import pika
from message_receiver import MessageReceiver
import threading
import sys

# config
agency = int(sys.argv[1]) or 1
mailbox_id = "agency{}".format(agency)

names_bindings = {
    'people': "#.ppl.#",
    'satellites': "#.sat.#",
    'cargo': "#.cgo.#"
}

exchange_name = 'business'
admin_ex_name = 'admin'

# administrative message queue:

receiver = MessageReceiver(mailbox_id)
receiver.bind_to_exchange(exchange_name,
                          ['#.'+mailbox_id+'.#'])
receiver.bind_to_exchange(admin_ex_name,
                          ['#.'+mailbox_id+'.#',
                           '#.agencies.#',
                           '#.all.#'])
admin_thread = threading.Thread(target=receiver.start_receiving)
admin_thread.start()

# exchange

connection = pika.BlockingConnection(
    pika.ConnectionParameters(host='localhost'))
channel = connection.channel()

channel.exchange_declare(
    exchange=exchange_name,
    exchange_type='topic')

# read input and send messages
order_no = 0

while True:
    order_name = input("Agency {}:".format(agency))
    if order_name in ['q', 'quit', 'exit']:
        break
    try:
        r_key = names_bindings[order_name]
        body = 'Order: agency {} no {} type {}'.format(agency, order_no, r_key)
        channel.basic_publish(exchange=exchange_name,
                              routing_key=r_key,
                              body=body)
        print('\tMessage sent!')
        order_no = order_no + 1
    except KeyError:
        print('Enter a valid operation [people|satellites|cargo]')
connection.close()
print("Agency {} closing...".format(agency))
