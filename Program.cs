using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;




namespace SchedulerCon
{
    class Program
    {
        static void Main(string[] args)
        {

            const string f = "TextFile1.txt"; //Place text file with schedule items in Debug folder

            // 1
            // Declare new List of type Tuple.            
            List<Tuple<string, int>> jlines = new List<Tuple<string, int>>();


            // 2
            // Use using StreamReader
            using (StreamReader r = new StreamReader(f))
            {
                // 3
                // Use while != null pattern for loop
                string line;
                while ((line = r.ReadLine()) != null)
                {
                    // 4
                    // Split each line on last instance of Colon
                    string[] SplitLine = line.Split(':');
                    string s = line;
                    string title = "";
                    string duration = "";
                    int idx = s.LastIndexOf(':');

                    if (idx != -1)
                    {
                        // First part save in string variable for title, second part in string variable for duration of meeting
                        title = s.Substring(0, idx);
                        duration = s.Substring(idx + 1);                     
                    }
                    
                   
                    //Extract only the numbers in duration string and convert into integer
                   
                    duration = Regex.Replace(duration, "[^0-9]+", string.Empty);
                    int durationInt = Int32.Parse(duration);

                    //Create and store separated title and duration values in Tuple
                    
                    Tuple<string, int> tuple = new Tuple<string, int>(title, durationInt);                    
                    jlines.Add(tuple);
                }
            }

            // 5
            // Loop through list and add up the durations to calculate number of sessions of 180 minutes each.

            int AddDuration = 0;
            int SumDurations = 0;
            DateTime today = DateTime.Today;
            DateTime startTime = today.AddHours(9);
            foreach (Tuple<string, int> s in jlines)
            {
                TimeSpan result = TimeSpan.FromMinutes(AddDuration);
                startTime = startTime.AddMinutes(AddDuration);
                AddDuration = s.Item2;
                SumDurations = SumDurations + s.Item2;


            }

            //Get the total number of sesssions
            Double NumSessions = Math.Ceiling(((double)SumDurations) / 180);
            
            //Call list splitting method
            var PartitionedList = Partition(jlines, (int)NumSessions);
            int SessionNo = 1;
            
            //Instatiate Start Time
            startTime = today.AddHours(9);



            int TrackNo = 1;

            //Loop through all the partitioned session lists and print out
            foreach (List<Tuple<string, int>> s in PartitionedList)
            {
                //int SumSessionDuration = 0;
                AddDuration = 0;
                int Sessionx = 0;
                if (SessionNo % 2 != 0)
                {

                    Console.WriteLine("#----------------------" + "Track   " + TrackNo + "--------------------------#");
                    TrackNo = TrackNo + 1;
                }

                if (SessionNo % 2 != 0)
                {
                    Sessionx = 1;
                    startTime = today.AddHours(9);
                    Console.WriteLine("  **************       " + "Session   " + Sessionx + "      ***************  ");
                }
                if (SessionNo % 2 == 0)
                {
                    Sessionx = 2;
                    startTime = today.AddHours(13);
                    Console.WriteLine("  **************       " + "Session   " + Sessionx + "      ***************  ");
                }





                Console.WriteLine();
                foreach (Tuple<string, int> t in s)
                {
                    
                    startTime = startTime.AddMinutes(AddDuration);
                    Console.WriteLine(string.Format("{0} {1} {2} min", startTime.ToString("hh:mm tt"), t.Item1, t.Item2.ToString()));
                    AddDuration = t.Item2;                

                }

                if (Sessionx ==2)
                {
                    DateTime netTime = today.AddHours(16);
                    string desc = "- Networking Event";
                    //Console.WriteLine(string.Format("{0} {1} {2} min", netTime.ToString("hh:mm tt"), desc));
                    Console.WriteLine(string.Format("{0} {1}", netTime.ToString("hh:mm tt"), desc));

                }
                Console.WriteLine();
                SessionNo = SessionNo + 1;
            }
        }

        public static List<Tuple<string, int>>[] Partition(List<Tuple<string, int>> list, int totalSessions)
        {   //Check if list is empty
            if (list == null)
                throw new ArgumentNullException("list");
            //Check if the value for total number of sessions/partitions will be less than 1
            if (totalSessions < 1)
                throw new ArgumentOutOfRangeException("totalSessions");

            //List array for storing the partitions
            List<Tuple<string, int>>[] partitions = new List<Tuple<string, int>>[totalSessions];

            int maxSize = (int)Math.Ceiling(list.Count / (double)totalSessions);
            int k = 0;
            // Create New list and order list by duration from highest to lowest
            var newList = list.OrderByDescending(x => x.Item2)
                  .ThenBy(x => x.Item1)
                  .ToList();
            //Alternate Between Highest and Lowest Durations in list for meetings to alternate between long and short meetings in sessions.
            var result = newList
                .Select((v, i) => new { Value = v, Index = i })
                .OrderBy(v => Math.Min(v.Index, Math.Abs((newList.Count - 1) - v.Index)))
                .Select(v => v.Value)
                .ToList();


            for (int i = 0; i < partitions.Length; i++)
            {

                partitions[i] = new List<Tuple<string, int>>();
                bool greaterThan180 = false;


                //Nested loop paritions creation
                for (int j = k; j < k + maxSize; j++)
                {
                    int InnerAddDuration = 0; //For checking sum of session durations does not exceed 180
                    if (greaterThan180 == true)
                    {
                        k = k - 1;

                    }
                    if (j >= result.Count)
                        break;
                    partitions[i].Add(result[j]);   //Add values to session

                    foreach (Tuple<string, int> w in partitions[i])
                        InnerAddDuration = InnerAddDuration + w.Item2; //increment and add to duration of session to check does not exceed180
                        greaterThan180 = false;
                    if (InnerAddDuration > 180)
                    {
                        partitions[i].Remove(result[j]);    //Remove value if duration of session exceeds 180
                        greaterThan180 = true;              //boolean to use outside the loop for decrementing indices and add to next session value greater than session duration of 180
                        j = j - 1;
                    }

                }

                k += maxSize;
            }

            return partitions;
        }
    }

}


