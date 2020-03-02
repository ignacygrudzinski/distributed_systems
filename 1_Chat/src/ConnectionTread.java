public class ConnectionTread implements Runnable {

    private int id;
    private String clientAddress;
    private int clientPort;
    ConnectionTread(int id, String address, int port){
        this.id = id;
        this.clientAddress = address;

    }

    @Override
    public void run() {

    }
}
