import pika


def callback(ch, method, properties, body):
    print(body.decode("utf-8"))


class MessageReceiver:
    def __init__(self, queue_name):
        self.queue_name = queue_name
        self.connection = pika.BlockingConnection(
            pika.ConnectionParameters(host='localhost'))
        self.channel = self.connection.channel()


    def bind_to_exchange(self, exchange_name, bindings):
        self.channel.exchange_declare(
            exchange=exchange_name,
            exchange_type='topic')
        self.channel.queue_declare(queue=self.queue_name)
        for topic in bindings:
            self.channel.queue_bind(
                exchange=exchange_name,
                queue=self.queue_name,
                routing_key=topic)

    def start_receiving(self):
        self.channel.basic_consume(
            queue=self.queue_name,
            on_message_callback=callback,
            auto_ack=True)
        self.channel.start_consuming()
