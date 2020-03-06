import javax.swing.*;
import java.awt.*;
import java.io.IOException;
import java.io.PrintWriter;
import java.net.Socket;
import java.util.Scanner;

public class ChatClient
{
    private String serverAddress;
    private int serverPort;

    private Scanner in;
    private PrintWriter out;

    JFrame frame = new JFrame("Chat");
    JTextField textField = new JTextField(50);
    JTextArea messageArea = new JTextArea(16, 50);

    ChatClient(String serverAddress, int serverPort)
    {
        this.serverAddress = serverAddress;
        this.serverPort = serverPort;

        textField.setEditable(true);
        messageArea.setEditable(false);

        frame.getContentPane().add(new JScrollPane(messageArea), BorderLayout.NORTH);
        frame.getContentPane().add(textField, BorderLayout.SOUTH);
        frame.pack();

        textField.addActionListener(e ->
        {
            String text = textField.getText();
            out.println(text);
            textField.setText("");
        });

    }


    private void run(){
    try(Socket socket = new Socket(this.serverAddress, this.serverPort))
        {

            //Close socket in a civilised manner on exit
            frame.addWindowListener(new java.awt.event.WindowAdapter() {
                @Override
                public void windowClosing(java.awt.event.WindowEvent windowEvent) {
                    try {
                        socket.close();
                    } catch (IOException e) {
                        e.printStackTrace();
                    }
                    //debug:
                    System.exit(0);
                }
            });

            //do
            in = new Scanner(socket.getInputStream());
            out = new PrintWriter(socket.getOutputStream(), true);

            while (in.hasNextLine())
            {
                String line = in.nextLine();
                messageArea.append(line + "\n");
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

    public static void main(String[] args)
    {
        int serverPort = 11000;
        String serverAddress = "127.0.0.1";
        ChatClient client = new ChatClient(serverAddress, serverPort);
        client.frame.setVisible(true);
        client.run();
    }
}