//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Markus Hofer">
//     Copyright (c) Markus Hofer
// </copyright>
// <summary>This program can receive data pages, which will be rendered on the console.</summary>
//-----------------------------------------------------------------------
namespace Aufgabe5Server
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Aufgabe5;

    /// <summary>
    /// This program can receive data pages, which will be rendered on the console.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// List of all records, which are currently saved by the program.
        /// </summary>
        private static List<Record> records;

        /// <summary>
        /// Instance of the record, which is displayed at the moment.
        /// </summary>
        private static Record currentlyDisplayedRecord;

        /// <summary>
        /// A boolean indicating if the rendering is currently activated or not.
        /// </summary>
        private static bool renderingActivated;

        /// <summary>
        /// This method represents the entry point of the program.
        /// </summary>
        /// <param name="args">Array of command line arguments.</param>
        private static void Main(string[] args)
        {
            Console.WindowHeight = 30;

            records = new List<Record>();

            renderingActivated = true;

            RecordReceiver server = new RecordReceiver(records);

            server.OnRecordsReceived += Server_OnRecordsReceived;

            server.Start();

            Thread thread = new Thread(new ThreadStart(HandleRecords));
            thread.Start();

            DrawMenu();

            ConsoleKeyInfo cki;

            while (true)
            {
                cki = Console.ReadKey(true);

                switch (cki.Key)
                {
                    case ConsoleKey.F1:
                        System.Environment.Exit(0);
                        break;
                    case ConsoleKey.F2:
                        renderingActivated = !renderingActivated;

                        if (renderingActivated)
                        {
                            Thread t = new Thread(new ThreadStart(HandleRecords));
                            t.Start();
                        }
                        else
                        {
                            currentlyDisplayedRecord = null;
                        }

                        DrawMenu();
                        break;
                }
            }
        }

        /// <summary>
        /// Draws the menu on the console.
        /// </summary>
        private static void DrawMenu()
        {
            Console.Clear();

            if (renderingActivated)
            {
                Console.WriteLine("[F1] Exit  [F2] Stop rendering");
            }
            else
            {
                Console.WriteLine("[F1] Exit  [F2] Start rendering");
            }
        }

        /// <summary>
        /// Gets called when the server received some records from a client.
        /// </summary>
        /// <param name="transferredRecords">A list of all received records.</param>
        private static void Server_OnRecordsReceived(List<Record> transferredRecords)
        {
            records.AddRange(transferredRecords);
        }

        /// <summary>
        /// Handles all records by checking their timestamp and duration and comparing them with the current time.
        /// </summary>
        private static void HandleRecords()
        {
            while (renderingActivated)
            {
                /*if (renderingActivated)
                {

                }*/

                for (int i = 0; i < records.Count; i++)
                {
                    // Does the time period of the record matches with the current time?
                    if (DateTime.Now >= records[i].Timestamp && records[i].Timestamp.AddSeconds(records[i].Duration) >= DateTime.Now)
                    {
                        // Is another record currently shown?
                        if (currentlyDisplayedRecord != null)
                        {
                            // Check if a time is reached, where a new record should be displayed. The old record will be cleared.
                            if (!currentlyDisplayedRecord.Equals(records[i]) && records[i].Timestamp > currentlyDisplayedRecord.Timestamp)
                            {
                                // If the old record should NOT be shown again when the new record expired before, uncomment the following statement.
                                // records.Remove(currentlyDisplayedRecord);
                                DrawMenu();

                                currentlyDisplayedRecord = records[i];
                                ShowRecord(currentlyDisplayedRecord);
                            }
                        }
                        else
                        {
                            currentlyDisplayedRecord = records[i];
                            ShowRecord(currentlyDisplayedRecord);
                        }                            
                    }
                    else if (DateTime.Now >= records[i].Timestamp.AddSeconds(records[i].Duration))
                    {
                        // The record is too old, so remove it. If it is shown at the moment, clear the screen.
                        if (currentlyDisplayedRecord != null && currentlyDisplayedRecord.ID == records[i].ID)
                        {
                            DrawMenu();
                            currentlyDisplayedRecord = null;
                        }

                        records.Remove(records[i]);
                    }
                }

                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Shows the page data and all additional information of the given record on the console.
        /// </summary>
        /// <param name="record">The given record.</param>
        private static void ShowRecord(Record record)
        {
            DrawMenu();
            Console.WriteLine("Available from {0}    ID: {1}    Duration: {2}", record.Timestamp.ToString(), record.ID, record.Duration);
            Console.WriteLine("            to {0}", record.Timestamp.AddSeconds(record.Duration));
            Console.Write("--------------------------------------------------------------------------------");

            record.RenderPageData(4);
        }
    }
}
