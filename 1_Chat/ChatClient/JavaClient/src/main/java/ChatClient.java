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

    private Scanner input;
    private PrintWriter output;

    private JFrame frame = new JFrame("Chat");
    private JTextArea area = new JTextArea(32, 80);
    private JTextField field = new JTextField(80);

    ChatClient(String serverAddress, int serverPort)
    {
        this.serverAddress = serverAddress;
        this.serverPort = serverPort;

        field.setEditable(true);
        area.setFont(new Font("monospaced", Font.TRUETYPE_FONT, 12));
        area.setEditable(false);

        frame.getContentPane().add(new JScrollPane(area), BorderLayout.NORTH);
        frame.getContentPane().add(field, BorderLayout.SOUTH);
        frame.pack();


        field.addActionListener(e ->
        {
            String text = field.getText();
            if(text.startsWith("!u")){
                output.print(ASCIIReader.getASCII() + "\r\0");
                output.flush();
                text = text.substring(2);
            }
            if (!text.isEmpty()){
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
                    //debug:
                    System.exit(0);
                }
            });

            //do

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

    public static void main(String[] args)
    {
        int serverPort = 11000;
        String serverAddress = "127.0.0.1";
        ChatClient client = new ChatClient(serverAddress, serverPort);
        client.frame.setVisible(true);
        client.run();
    }
}