using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiQueueModels
{
    public class SimulationCase
    {
        public SimulationCase()
        {
            this.AssignedServer = new Server();
        }

        public int CustomerNumber { get; set; }
        public int RandomInterArrival { get; set; }
        public int InterArrival { get; set; }
        public int ArrivalTime { get; set; }
        public int RandomService { get; set; }
        public int ServiceTime { get; set; }
        public Server AssignedServer { get; set; }
        public int serverindex { get; set; }
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        public int TimeInQueue { get; set; }
        public SimulationCase(int id)
        {
            CustomerNumber = id;
            RandomInterArrival = 0;
            InterArrival = 0;
            ArrivalTime = 0;
            RandomService = 0;
            ServiceTime = 0;
            this.AssignedServer = new Server(); ;
            StartTime = 0;
            EndTime = 0;


        }
    }
}
