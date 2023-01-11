using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiQueueModels
{
    public class Server
    {
        public int ID { get; set; }
        public decimal IdleProbability { get; set; }
        public decimal AverageServiceTime { get; set; } 
        public decimal Utilization { get; set; }

        public List<TimeDistribution> TimeDistribution = new List<TimeDistribution>();

       
        public int FinishTime { get; set; }
        public int TotalWorkingTime { get; set; }

        public decimal customers { get; set; }
        public decimal idle { get; set; }

        // x,y variables for graph 
        public List<int> x;

        public List<int> y;
        public Server(int id)
        {
            ID = id;

            IdleProbability = 0;

            AverageServiceTime = 0;

            Utilization = 0;

            FinishTime = 0;
            
            TotalWorkingTime = 0;

             customers = 0;

            idle = 0;
            // x,y variables for graph 
            x = new List<int>();
            y = new List<int>();
        }

        public Server()
        {

        }
    }
}
