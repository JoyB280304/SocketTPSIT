using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Socket_4I
{
    /// <summary>
    /// Logica di interazione per MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Socket socket = null;
        DispatcherTimer dTimer = null;
        public MainWindow()
        {
            InitializeComponent();

            //Imposto di default le socket di entrata ed uscita 
            txtSocketIn.Text = "11000";
            txtSocketOut.Text = "10000";

            //Creo una nuova istanza della classe Socket UDP
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp); 

            //Associo al socket una porta, in questo caso la porta 11000
            IPAddress local_address = IPAddress.Any;
            IPEndPoint local_endpoint = new IPEndPoint(local_address, int.Parse(txtSocketIn.Text));

            socket.Bind(local_endpoint);

            //Setto le impostazioni del socket, in questo caso tolgo il blocco e abilito il broadcast
            socket.Blocking = false;
            socket.EnableBroadcast = true;

            //Creo il timer per evitare che le informazioni vengano mandate nello stesso istante
            dTimer = new DispatcherTimer();
            //Imposto il tempo che il timer avrà, in questo caso 250 ms
            dTimer.Interval = new TimeSpan(0, 0, 0, 0, 250);

            //Qua avviene l'impostazione del metodo "aggiornamento_dTimer" alla fine dell'intervallo di tempo
            dTimer.Tick += new EventHandler(aggiornamento_dTimer);

            //Avvio il timer
            dTimer.Start();
        }

        //Creazione del metodo per l'aggiornamento del timer allo scadere del tempo
        private void aggiornamento_dTimer(object sender, EventArgs e)
        {
            int nBytes = 0;

            //Controlla che non ci siano ulteriori dati in arrivo al socket in modo tale da potere cominciare con l'esecuzione del metodo
            if ((nBytes = socket.Available) > 0)
            {
                //Ricezione dei caratteri in attesa
                byte[] buffer = new byte[nBytes];
                EndPoint remoreEndPoint = new IPEndPoint(IPAddress.Any, 0);
                nBytes = socket.ReceiveFrom(buffer, ref remoreEndPoint);

                //Ricavo l'IP del mittente
                string from = ((IPEndPoint)remoreEndPoint).Address.ToString();

                //Decodifico i dati ricevuti
                string messaggio = Encoding.UTF8.GetString(buffer, 0, nBytes);

                //Aggiungo il messaggio alla lista dei messagi nella gui
                lstMessaggi.Items.Add(from+": "+messaggio);
            }
        }

        //Metodo sul click del bottone "Invia"
        private void btnInvia_Click(object sender, RoutedEventArgs e)
        {
            IPAddress remote_address = null;
            if(rbBroadcast.IsChecked == true)
            {
                remote_address = IPAddress.Broadcast; //Se lo voglio broadcast uso questo IPAddress
            }
            else if(rbClient.IsChecked == true)
            {
                if (txtTo.Text == "0.0.0.0")
                {
                    MessageBox.Show("Devi impostare un ip di destinazione!");
                }

                //Di default si setta l'ip dell'indirizzo locale (192.168.1.x), per trovarlo apro una prompt dei comandi e utilizzo il comando "ipconfig"
                remote_address = IPAddress.Parse(txtTo.Text); //txtTo.Text deve contenere l'indirizzo ip (ipv4 locale) del client a cui inviamo il messaggio
            }

            //Associo come porta di destinazione la 10000 e come ip quello impostato in precedenza
            IPEndPoint remote_endpoint = new IPEndPoint(remote_address, int.Parse(txtSocketOut.Text));

            //txt.Messaggio.Text deve contenere il messaggio scritto da inviare
            byte[] messaggio = Encoding.UTF8.GetBytes(txtMessaggio.Text); 

            //Invio il pacchetto al destinatario specificato nella variabile "remote_endpoint"
            socket.SendTo(messaggio, remote_endpoint);
        }
    }
}