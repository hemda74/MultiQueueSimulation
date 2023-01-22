using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MultiQueueModels;
using MultiQueueTesting;
using System.IO;
using System.Windows.Forms.DataVisualization.Charting;

namespace MultiQueueSimulation
{
    public partial class Form1 : Form
    {
        
        public Form1()
        {
            InitializeComponent();
            
        }
        // define SimulationSystem object & PerformanceMeasures object 
        public static SimulationSystem mysystem= new SimulationSystem();
        PerformanceMeasures per = new PerformanceMeasures();
        int total_run = 0;

        
        private void Form1_Load(object sender, EventArgs e)
        {
     
            readData_buildsmallTable();
            build_SimulationTable();
            dataGridView1.DataSource = mysystem.SimulationTable;
            //show simulation table
            add_servers_combobox();
            string testonobj = TestingManager.Test(mysystem, Constants.FileNames.TestCase1);

            MessageBox.Show(testonobj);
            
        }
        // reading data from test cases file
        public void readData_buildsmallTable()
        {
            FileStream fs = new FileStream("TestCase1.txt", FileMode.Open);
              StreamReader  sr  =  new  StreamReader(fs);
            while (sr.Peek() != -1)
            {
                // cheking and categerios the input parameters from test case file  then assgine this parameters to simulation system variables  
                string my_Line = sr.ReadLine();
                if (my_Line == ""|| my_Line == null)
                {
                    continue;
                }
                // getting  num of servers 
                else if (my_Line == "NumberOfServers")
                {
                    mysystem.NumberOfServers = int.Parse(sr.ReadLine());
                }
                // getting stooping number
                else if (my_Line == "StoppingNumber")
                {
                    mysystem.StoppingNumber = int.Parse(sr.ReadLine());
                }
                //getting  stoping criteria 
                else if (my_Line == "StoppingCriteria")
                    // here i have checked stoping criteria type 
                {
                    int x = int.Parse(sr.ReadLine());
                    // if it == 1 then it will be num of customers 
                    if (x ==(int) Enums.StoppingCriteria.NumberOfCustomers)
                    {
                        mysystem.StoppingCriteria = Enums.StoppingCriteria.NumberOfCustomers;
                    }
                    else
                    {
                        // if it == 2 then it will be end sim time 
                        mysystem.StoppingCriteria = Enums.StoppingCriteria.SimulationEndTime;
                    }
                }
                // here i have checked the selection method 
                else if (my_Line == "SelectionMethod")
                {
                    int y = int.Parse(sr.ReadLine());
                    // if 1 then it will be highest priority
                    if (y == (int)Enums.SelectionMethod.HighestPriority)
                    {
                        mysystem.SelectionMethod = Enums.SelectionMethod.HighestPriority;
                    }
                    // if 2 then it will be random 
                    else if (y == (int)Enums.SelectionMethod.Random)
                    {
                        mysystem.SelectionMethod = Enums.SelectionMethod.Random;
                    }
                    else
                    {
                        mysystem.SelectionMethod = Enums.SelectionMethod.LeastUtilization;
                    }
                }
                else if (my_Line == "InterarrivalDistribution")
                //here i have calculate commulative Probability for multi ranges 
                {
                    decimal lastprop = 0;
                    while (true)
                    {

                        string line = sr.ReadLine();
                        if (line == "" || line == null) //stop condition
                        {
                            break;
                        }
                        string[] arr = line.Split(',');
                        TimeDistribution row = new TimeDistribution();
                        row.Time = int.Parse(arr[0]);
                        decimal pro = decimal.Parse(arr[1]);
                        row.Probability = pro;
                        row.CummProbability = lastprop + pro;
                        row.MinRange = (int)(lastprop * 100 + 1);
                        lastprop = row.CummProbability;                               
                        row.MaxRange = (int)(lastprop * 100); 
                        mysystem.InterarrivalDistribution.Add(row);
                    }
                }
                // calculate Time Distribution for each server
                else if (my_Line.Contains("ServiceDistribution_Server"))
                {
                    for (int i = 0; i < mysystem.NumberOfServers; i++)
                    {   
                         Server servobj = new Server(i+1);
                        servobj.FinishTime = 0;
                          
                        decimal lastprop = 0;
                        while (true)
                        {
                            string line = sr.ReadLine();
                            if ( line  ==  ""  ||  line  ==  null )
                            {
                               sr.ReadLine();

                                break;
                            }

                            string[] arr = line.Split(',');
                            
                            TimeDistribution row = new TimeDistribution();
                            
                            row.Time = int.Parse(arr[0]);
                            
                            decimal pro = decimal.Parse(arr[1]);
                            
                            row.Probability = pro;
                            
                            row.CummProbability = lastprop + pro;
                            
                            row.MinRange = (int)(lastprop * 100 + 1);
                            
                            lastprop = row.CummProbability;
                            
                            row.MaxRange = (int)(lastprop * 100);
                            
                            servobj.TimeDistribution.Add(row);
                        }

                        mysystem.Servers.Add(servobj);
                    }

                }

            }
            fs.Close();
        }
        void build_SimulationTable()
        {
            int cid = 0;
            
            per.AverageWaitingTime = 0;
            
            per.WaitingProbability = 0;
            
            per.MaxQueueLength = 0;
            
            List<int> queu = new List<int>();

            Random rnd = new Random();
            
            while (true)
            {
                cid++;
                if (mysystem.StoppingCriteria == Enums.StoppingCriteria.NumberOfCustomers)
                {
                    if (stop(cid))

                        break;
                }
                
                SimulationCase cus_obj = new SimulationCase(cid);
                
                cus_obj.RandomInterArrival = rnd.Next(1, 101);
                
                cus_obj.InterArrival = get_arrival_time(cus_obj.RandomInterArrival);
                
                if (cid == 1)
                {
                    cus_obj.ArrivalTime = 0;
                }
                else
                {
                    cus_obj.ArrivalTime = cus_obj.InterArrival + mysystem.SimulationTable[cid - 2].ArrivalTime;
                }
                if (mysystem.StoppingCriteria == Enums.StoppingCriteria.SimulationEndTime)
                {
                    if (stop(cus_obj.ArrivalTime))
                        break;
                }
                cus_obj.RandomService = rnd.Next(1, 101);
                cus_obj.TimeInQueue = -1;
                if (mysystem.SelectionMethod == Enums.SelectionMethod.HighestPriority)
                {
                    for (int i = 0; i < mysystem.NumberOfServers; i++)
                    {
                        if (mysystem.Servers[i].FinishTime <= cus_obj.ArrivalTime)//server
                        {
                            mysystem.Servers[i].idle += Math.Abs(cus_obj.ArrivalTime - mysystem.Servers[i].FinishTime);
                            cus_obj.ServiceTime = get_service_time(cus_obj.RandomService, mysystem.Servers[i]);
                            cus_obj.TimeInQueue = 0;
                            
                            mysystem.Servers[i].FinishTime = cus_obj.ArrivalTime + cus_obj.ServiceTime;
                            
                            cus_obj.StartTime = cus_obj.ArrivalTime;
                            
                            cus_obj.EndTime = cus_obj.ArrivalTime + cus_obj.ServiceTime;
                            
                            cus_obj.AssignedServer = mysystem.Servers[i];
                            
                            mysystem.Servers[i].TotalWorkingTime += cus_obj.ServiceTime;
                            
                            mysystem.Servers[i].customers++;
                           
                            cus_obj.serverindex = mysystem.Servers[i].ID;

                            break;
                        }
                    }
                    if (cus_obj.TimeInQueue == -1)
                    {
                        
                        for (int i = 0; i < queu.Count; i++)
                        {                           
                            if (cus_obj.ArrivalTime >= queu[i])

                                queu.RemoveAt(i);
                        }
                        
                        int mn = 1000000000, ind = 0;
                        
                        
                        for (int i = 0; i < mysystem.NumberOfServers; i++)
                        {
                            if (mysystem.Servers[i].FinishTime < mn)
                            {
                                mn = mysystem.Servers[i].FinishTime;

                                ind = i;
                            }
                        }
                        //new start time of service(finish time old)
                        queu.Add(mn);

                        cus_obj.ServiceTime = get_service_time(cus_obj.RandomService, mysystem.Servers[ind]);

                        cus_obj.TimeInQueue = mn - cus_obj.ArrivalTime;

                        mysystem.Servers[ind].FinishTime = mn + cus_obj.ServiceTime;

                        cus_obj.StartTime = mn;

                        cus_obj.EndTime = mn + cus_obj.ServiceTime;

                        cus_obj.AssignedServer = mysystem.Servers[ind];

                        per.AverageWaitingTime += cus_obj.TimeInQueue;

                        per.WaitingProbability++;

                        mysystem.Servers[ind].TotalWorkingTime += cus_obj.ServiceTime;

                        mysystem.Servers[ind].customers++;

                        cus_obj.serverindex = mysystem.Servers[ind].ID;

                    }
                    per.MaxQueueLength = Math.Max(per.MaxQueueLength, queu.Count);
                }
                else if (mysystem.SelectionMethod == Enums.SelectionMethod.LeastUtilization)
                {

                    List<Server> sers = new System.Collections.Generic.List<Server>();

                    decimal mn = 10000000;

                    int mnn = 10000000;

                    foreach (Server ser in mysystem.Servers)
                    {
                        ser.Utilization = (decimal)(ser.TotalWorkingTime);

                        if (ser.FinishTime <= cus_obj.ArrivalTime)
                        {
                            mn = Math.Min(mn, ser.Utilization);

                            sers.Add(ser);
                        }
                        mnn = Math.Min(mnn, ser.FinishTime);

                    }
                    if (sers.Count > 0)
                    {
                        int ind = 0;

                        foreach (Server ser in sers)
                        {
                            if (ser.Utilization == mn)
                            {
                                ind = ser.ID - 1;

                                break;
                            }
                        }
                        mysystem.Servers[ind].idle += Math.Abs(cus_obj.ArrivalTime - mysystem.Servers[ind].FinishTime);
                        cus_obj.ServiceTime = get_service_time(cus_obj.RandomService, mysystem.Servers[ind]);
                        cus_obj.TimeInQueue = 0;

                        
                        mysystem.Servers[ind].FinishTime = cus_obj.ArrivalTime + cus_obj.ServiceTime;
                        
                        cus_obj.StartTime = cus_obj.ArrivalTime;
                        
                        cus_obj.EndTime = cus_obj.ArrivalTime + cus_obj.ServiceTime;
                        
                        cus_obj.AssignedServer = mysystem.Servers[ind];
                        
                        mysystem.Servers[ind].TotalWorkingTime += cus_obj.ServiceTime;
                        
                        mysystem.Servers[ind].customers++;
                        
                        cus_obj.serverindex = mysystem.Servers[ind].ID;


                    }
                    else
                    {
                        sers = new System.Collections.Generic.List<Server>();

                        mn = 10000000;

                        foreach (Server ser in mysystem.Servers)
                        {
                            if (ser.FinishTime == mnn)
                            {
                                mn = Math.Min(mn, ser.Utilization);

                                sers.Add(ser);
                            }
                        }
                        int ind = 0;
                        foreach (Server ser in sers)
                        {
                            if (ser.Utilization == mn)
                            {
                                ind = ser.ID - 1;

                                break;
                            }
                        }
                        for (int i = 0; i < queu.Count; i++)
                        {
                            if (cus_obj.ArrivalTime >= queu[i])

                                queu.RemoveAt(i);
                        }

                        queu.Add(mnn);
                        cus_obj.ServiceTime = get_service_time(cus_obj.RandomService, mysystem.Servers[ind]);
                        cus_obj.TimeInQueue = mysystem.Servers[ind].FinishTime - cus_obj.ArrivalTime;

                        mysystem.Servers[ind].FinishTime = mnn + cus_obj.ServiceTime;
                        cus_obj.StartTime = mnn;

                        cus_obj.EndTime = mnn + cus_obj.ServiceTime;

                        cus_obj.AssignedServer = mysystem.Servers[ind];

                        per.AverageWaitingTime += cus_obj.TimeInQueue;
                        per.WaitingProbability++;

                        mysystem.Servers[ind].TotalWorkingTime += cus_obj.ServiceTime;

                        mysystem.Servers[ind].customers++;

                        cus_obj.serverindex = mysystem.Servers[ind].ID;
                    }

                    per.MaxQueueLength = Math.Max(per.MaxQueueLength, queu.Count);

                }
                else
                {

                    Random rnd1 = new Random();

                    List<Server> sers = new System.Collections.Generic.List<Server>();

                    decimal mn = 10000000;

                    int mnn = 10000000;

                    foreach (Server ser in mysystem.Servers)
                    {
                        if (ser.FinishTime <= cus_obj.ArrivalTime)
                        {
                            sers.Add(ser);
                        }
                        mnn = Math.Min(mnn, ser.FinishTime);// min finish time y3ny 
                    }
                    if (sers.Count > 0)
                    {
                        int ind = rnd1.Next(0, sers.Count);

                        ind = sers[ind].ID - 1;

                        mysystem.Servers[ind].idle += Math.Abs(cus_obj.ArrivalTime - mysystem.Servers[ind].FinishTime);
                        cus_obj.ServiceTime = get_service_time(cus_obj.RandomService, mysystem.Servers[ind]);
                        cus_obj.TimeInQueue = 0;

                        mysystem.Servers[ind].FinishTime = cus_obj.ArrivalTime + cus_obj.ServiceTime;
                        cus_obj.StartTime = cus_obj.ArrivalTime;

                        cus_obj.EndTime = cus_obj.ArrivalTime + cus_obj.ServiceTime;

                        cus_obj.AssignedServer = mysystem.Servers[ind];

                        mysystem.Servers[ind].TotalWorkingTime += cus_obj.ServiceTime;

                        mysystem.Servers[ind].customers++;

                        cus_obj.serverindex = mysystem.Servers[ind].ID;
                    }
                    else
                    {
                        sers = new System.Collections.Generic.List<Server>();

                        foreach (Server ser in mysystem.Servers)
                        {
                            if (ser.FinishTime == mnn)
                            {
                                sers.Add(ser);
                            }
                        }
                        int ind = rnd1.Next(0, sers.Count);

                        ind = sers[ind].ID - 1;

                        for (int i = 0; i < queu.Count; i++)
                        {
                            if (cus_obj.ArrivalTime >= queu[i])

                                queu.RemoveAt(i);
                        }

                        queu.Add(mnn);
                        cus_obj.ServiceTime = get_service_time(cus_obj.RandomService, mysystem.Servers[ind]);
                        cus_obj.TimeInQueue = mysystem.Servers[ind].FinishTime - cus_obj.ArrivalTime;
                        mysystem.Servers[ind].FinishTime = mnn + cus_obj.ServiceTime;

                        cus_obj.StartTime = mnn;

                        cus_obj.EndTime = mnn + cus_obj.ServiceTime;
                        
                        cus_obj.AssignedServer = mysystem.Servers[ind];
                        
                        per.AverageWaitingTime += cus_obj.TimeInQueue;
                        
                        per.WaitingProbability++;
                        
                        mysystem.Servers[ind].TotalWorkingTime += cus_obj.ServiceTime;
                        
                        mysystem.Servers[ind].customers++;
                        
                        cus_obj.serverindex = mysystem.Servers[ind].ID;
                    }

                    per.MaxQueueLength = Math.Max(per.MaxQueueLength, queu.Count);

                }
                // graph for displaying working time for each server 
                for (int t = cus_obj.StartTime+1; cus_obj.StartTime <= 20 && t <= cus_obj.EndTime; t++)
                {
                    mysystem.Servers[cus_obj.AssignedServer.ID - 1].x.Add(t);

                    mysystem.Servers[cus_obj.AssignedServer.ID - 1].y.Add(1);
                }
                
                mysystem.SimulationTable.Add(cus_obj);

                total_run = Math.Max(total_run, cus_obj.EndTime);
                
            }
            //calculating  system performance
            per.WaitingProbability = per.WaitingProbability / mysystem.StoppingNumber;

            per.AverageWaitingTime = per.AverageWaitingTime / mysystem.StoppingNumber;
            mysystem.PerformanceMeasures = per;

            textBox4.Text = per.WaitingProbability.ToString();

            textBox5.Text = per.AverageWaitingTime.ToString();

            textBox6.Text = per.MaxQueueLength.ToString();

            calc_servers_performance();

        }
        // calculate performance of servers
        public void calc_servers_performance() 
        {
            foreach (Server ser in mysystem.Servers)
            {

                ser.AverageServiceTime = (decimal)(ser.TotalWorkingTime) / (Math.Max(1, ser.customers));

                ser.IdleProbability = ((total_run - ser.FinishTime) + (decimal)ser.idle) / total_run;
                //total_run-(decimal)(ser.TotalWorkingTime)
                ser.Utilization = (decimal)(ser.TotalWorkingTime) / total_run;
            }

        }
        // get arrival time for customers
        public int get_arrival_time(int x) 
        {
            foreach (TimeDistribution tim in mysystem.InterarrivalDistribution)
            {
                if (x >= tim.MinRange && x <= tim.MaxRange)
                {
                    return tim.Time;
                }
            }
            return 0;
        }
        // get service time for servers
        public int get_service_time(int x, Server ser) 
        {
            foreach (TimeDistribution tim in ser.TimeDistribution)
            {
                if (x >= tim.MinRange && x <= tim.MaxRange)
                {
                    return tim.Time;
                }
            }
            return 0;
        }
        public bool stop(int x)
        {
            return x > mysystem.StoppingNumber;
        }
        public void add_servers_combobox()
        {
            for (int s = 0; s < mysystem.NumberOfServers; s++)
            {
                comboBox1.Items.Add(s + 1);
            }
           
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
           
            int ind = int.Parse(comboBox1.SelectedItem.ToString());

            MultiQueueModels.Server ser = mysystem.Servers[ind - 1];

            var series = new Series("Server " + comboBox1.SelectedItem.ToString());

            chart1.Series.Clear();
           
            series.Points.DataBindXY(ser.x, ser.y);

            chart1.Series.Clear();

            chart1.Series.Add(series);

            chart1.Series["Server " + comboBox1.SelectedItem.ToString()]["PointWidth"] = "0.5";

            textBox1.Text = (mysystem.Servers[ind - 1].AverageServiceTime).ToString();

            textBox2.Text = (mysystem.Servers[ind - 1].IdleProbability).ToString();

            textBox3.Text = (mysystem.Servers[ind - 1].Utilization).ToString();   

        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
        private void button1_Click(object sender, EventArgs e)
        {

        }
        private void label1_Click(object sender, EventArgs e)
        {

        }
        private void chart1_Click(object sender, EventArgs e)
        {

        }
    }
}
