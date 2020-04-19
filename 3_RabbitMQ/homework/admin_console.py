#!/usr/bin/env python
import pika
from message_receiver import MessageReceiver
import threading


business_ex_name = 'business'
admin_ex_name = 'admin'

# exchange

connection = pika.BlockingConnection(
    pika.ConnectionParameters(host='localhost'))
channel = connection.channel()

channel.exchange_declare(
    exchange=admin_ex_name,
    exchange_type='topic')

# sniffer queue:
receiver = MessageReceiver('admin_inbox')
receiver.bind_to_exchange(business_ex_name, ['#'])
admin_thread = threading.Thread(target=receiver.start_receiving)
admin_thread.start()


# read input and send messages
while True:
    command = input("Admin console: ")
    if command in ['q', 'quit', 'exit']:
        break
    try:
        [r_key, msg] = command.split(" ", 1)
        body = "Admin to {}: {}".format(r_key, msg)
        channel.basic_publish(exchange=admin_ex_name,
                              routing_key=r_key,
                              body=body)
        print('\tSent Message:\ntopic: {}\tbody: {}'.format(r_key, msg))
    except ValueError:
        print("Usage: [provider#|agency#|providers|agencies|all] message")
connection.close()
print("Admin console closing")
