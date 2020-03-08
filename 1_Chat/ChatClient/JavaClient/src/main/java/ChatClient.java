import javax.swing.*;
import java.awt.*;
import java.awt.event.WindowEvent;
import java.io.IOException;
import java.io.PrintWriter;
import java.net.*;
import java.nio.charset.StandardCharsets;
import java.util.Scanner;

//TODO: make connection-closing exits and stuff (move exit to another method maybe)


public class ChatClient
{

    private String serverAddress;
    private int serverPort;
    private String multicastGroup;
    private int multicastPort;

    private Scanner input;
    private PrintWriter output;

    private DatagramSocket socketUDP;
    private MulticastSocket socketMTC;

    private JFrame frame = new JFrame("Client Program");
    private JTextArea area = new JTextArea(32, 80);
    private JTextField field = new JTextField(80);

    public static void main(String[] args)
    {
        int serverPort = 11000;
        String serverAddress = "127.0.0.1";
        String multicastGroup = "224.2.2.4";
        int multicastPort = 6789;
        ChatClient client = new ChatClient(serverAddress, serverPort, multicastGroup, multicastPort);
        client.frame.setVisible(true);
        client.run();
    }

    private ChatClient(String serverAddress, int serverPort, String multicastGroup, int multicastPort)
    {
        this.serverAddress = serverAddress;
        this.serverPort = serverPort;
        this.multicastGroup = multicastGroup;
        this.multicastPort = multicastPort;

        field.setEditable(true);
        area.setFont(new Font("monospaced", Font.PLAIN, 12));
        area.setEditable(false);

        frame.getContentPane().add(new JScrollPane(area), BorderLayout.NORTH);
        frame.getContentPane().add(field, BorderLayout.SOUTH);
        frame.pack();


        field.addActionListener(e ->
        {
            String text = field.getText();
            if(text.startsWith("!u ")){
                writeDatagram(text.substring(3), socketUDP);
            } else if(text.startsWith("!m ")){
                writeDatagram(text.substring(3), socketMTC);
            }
            else if (text.startsWith("!q")){
                frame.dispatchEvent(new WindowEvent(frame, WindowEvent.WINDOW_CLOSING));
            }
            else if (text.startsWith("!a")){
                writeDatagram(ASCIIReader.getASCII(), socketUDP);
            }
            else if (!text.isEmpty()){
                output.println(text);
                output.flush();
            }
            field.setText("");
        });

    }

    private void print(String msg){
        String trimmed = msg.trim();
        if (! trimmed.isEmpty()){
            area.append(trimmed + '\n');
        }
    }

    private void run(){
    try(Socket socket = new Socket(this.serverAddress, this.serverPort))
        {

            input = new Scanner(socket.getInputStream());
            output = new PrintWriter(socket.getOutputStream(), true);

            //Initialize other sockets if TCP connection is successful
            socketUDP = new DatagramSocket(socket.getLocalPort());
            socketUDP.connect(InetAddress.getByName(this.serverAddress), this.serverPort);
            socketMTC = new MulticastSocket(multicastPort);
            socketMTC.joinGroup(InetAddress.getByName(multicastGroup));

            //Close socket input a civilised manner on exit
            frame.addWindowListener(new java.awt.event.WindowAdapter() {
                @Override
                public void windowClosing(java.awt.event.WindowEvent windowEvent) {
                    try {
                        output.println("!q");
                        socket.close();
                        output.close();
                        input.close();
                    } catch (IOException e) {
                        e.printStackTrace();
                    }
                    System.exit(0);
                }
            });

            new Thread(() -> {
                while (true)
                    print(readDatagram(socketUDP));
            }).start();

            new Thread(() -> {
                while (true)
                    print(readDatagram(socketMTC));
            }).start();


            while (input.hasNextLine())
            {
                print(input.nextLine());
            }
        }
        catch (IOException e)
        { e.printStackTrace(); }
        finally
        {
            frame.setVisible(false);
            frame.dispose();
        }
    }

    private void writeDatagram(String message, DatagramSocket dSocket) {
        try {
            byte[] buffer = (message + "\n\r").getBytes(StandardCharsets.UTF_8);
            DatagramPacket packet = new DatagramPacket(buffer, 0, buffer.length);
            if (dSocket instanceof MulticastSocket){
                packet.setAddress(InetAddress.getByName(multicastGroup));
                packet.setPort(multicastPort);
            }
            dSocket.send(packet);
        } catch (Exception e) {
            e.printStackTrace();
            frame.dispatchEvent(new WindowEvent(frame, WindowEvent.WINDOW_CLOSING));
        }

    }



    private String readDatagram(DatagramSocket dSocket) {
        try {
            byte[] buffer = new byte[1024];
            DatagramPacket packet = new DatagramPacket(buffer, 0, buffer.length);
            dSocket.receive(packet);
            return new String(packet.getData(),
                    StandardCharsets.UTF_8);
        } catch (Exception e) {
            e.printStackTrace();
            frame.dispatchEvent(new WindowEvent(frame, WindowEvent.WINDOW_CLOSING));
        }
        return "";
    }


}