import javax.swing.*;
import java.awt.*;
import java.awt.event.WindowEvent;
import java.io.IOException;
import java.io.PrintWriter;
import java.io.UnsupportedEncodingException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.Socket;
import java.nio.charset.StandardCharsets;
import java.util.Scanner;

public class ChatClient
{

    private String serverAddress;
    private int serverPort;

    private Scanner input;
    private PrintWriter output;

    private DatagramSocket socketUDP;

    private JFrame frame = new JFrame("Chat");
    private JTextArea area = new JTextArea(32, 80);
    private JTextField field = new JTextField(80);

    public static void main(String[] args)
    {
        int serverPort = 11000;
        String serverAddress = "127.0.0.1";
        String multicastGroup = "224.2.2.4";
        ChatClient client = new ChatClient(serverAddress, serverPort);
        client.frame.setVisible(true);
        client.run();
    }

    private ChatClient(String serverAddress, int serverPort)
    {
        this.serverAddress = serverAddress;
        this.serverPort = serverPort;

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
//                output.print(ASCIIReader.getASCII() + "\r\0");
//                output.flush();
                writeToUDP(text.substring(3));
            }
            else if (text.startsWith("!q")){
                frame.dispatchEvent(new WindowEvent(frame, WindowEvent.WINDOW_CLOSING));
            }
            else if (text.startsWith("!a")){
                writeToUDP(ASCIIReader.getASCII());
            }
            else if (!text.isEmpty()){
                output.println(text);
                output.flush();
            }
            field.setText("");
        });

    }


    private void run(){
    try(Socket socket = new Socket(this.serverAddress, this.serverPort))
        {

            input = new Scanner(socket.getInputStream());
            output = new PrintWriter(socket.getOutputStream(), true);

            socketUDP = new DatagramSocket(socket.getLocalPort());
            socketUDP.connect(InetAddress.getByName(this.serverAddress), this.serverPort);


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

            Runnable udpChannel = () -> {
                while (true)
                    area.append(readFromUDP() + '\n');
            };

            Thread t = new Thread(udpChannel);
            t.start();

            while (input.hasNextLine())
            {
                String line = input.nextLine();
                area.append(line + "\n");
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

    private void writeToUDP(String message) {
        try {
            byte[] buffer = (message + "\n\r").getBytes(StandardCharsets.UTF_8);
            socketUDP.send(new DatagramPacket(buffer, 0, buffer.length));
        } catch (Exception e) {
            e.printStackTrace();
            frame.dispatchEvent(new WindowEvent(frame, WindowEvent.WINDOW_CLOSING));
        }

    }

    private String readFromUDP() {
        try {
            byte[] buffer = new byte[1024];
            DatagramPacket packet = new DatagramPacket(buffer, 0, buffer.length);
            socketUDP.receive(packet);
            return new String(packet.getData(),
                    StandardCharsets.UTF_8);
        } catch (Exception e) {
            e.printStackTrace();
            frame.dispatchEvent(new WindowEvent(frame, WindowEvent.WINDOW_CLOSING));
        }
        return "";
    }

}