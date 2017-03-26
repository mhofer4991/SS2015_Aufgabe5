//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Markus Hofer">
//     Copyright (c) Markus Hofer
// </copyright>
// <summary>The user can use this program to create, edit and delete records. He can also export and import records and connect as a client to a server.</summary>
//-----------------------------------------------------------------------
namespace Aufgabe5
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// The user can use this program to create, edit and delete records. He can also export and import records and connect as a client to a server.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// A list of all currently available records.
        /// </summary>
        private static List<Record> availableRecords;

        /// <summary>
        /// This method represents the entry point of the program.
        /// </summary>
        /// <param name="args">Array of command line arguments.</param>
        private static void Main(string[] args)
        {
            Console.WindowHeight = 30;

            availableRecords = new List<Record>();
            availableRecords.Add(Record.GetEmptyRecord(0));

            bool exiting = false;
            ConsoleKey key;

            while (!exiting)
            {
                DrawMenu();

                key = Console.ReadKey().Key;

                switch (key)
                {
                    case ConsoleKey.F7:
                        System.Environment.Exit(0);
                        break;
                    case ConsoleKey.F1:
                        availableRecords.Add(CreateRecord());
                        break;
                    case ConsoleKey.F2:
                        EditRecord();
                        break;
                    case ConsoleKey.F3:
                        DeleteRecord();
                        break;
                    case ConsoleKey.F4:
                        ExportRecords();
                        break;
                    case ConsoleKey.F5:
                        ImportRecords();
                        break;
                    case ConsoleKey.F6:
                        ConnectToServer();
                        break;
                }
            }
        }

        /// <summary>
        /// Draws the available options on the menu.
        /// </summary>
        private static void DrawMenu()
        {
            Console.Clear();
            Console.WriteLine("\n [F1] Create record");
            Console.WriteLine("\n [F2] Edit record");
            Console.WriteLine("\n [F3] Delete record");
            Console.WriteLine("\n [F4] Export records");
            Console.WriteLine("\n [F5] Import records");
            Console.WriteLine("\n [F6] Connect to server");
            Console.WriteLine("\n [F7] Exit");
        }

        /// <summary>
        /// Creates a new record by prompting the user to enter values.
        /// </summary>
        /// <returns>A new record.</returns>
        private static Record CreateRecord()
        {
            Console.Clear();
            Console.WriteLine("\n [F2] Create record");

            string input;
            Record newRecord;

            int suggestedID = GetNewID();
            int newID = suggestedID;

            // The user use the suggested ID by entering only whitespaces.
            Console.WriteLine("\n > You can either enter your own ID or use the suggested one by entering\n   only whitespaces.");

            // Prompt the user to enter a new ID.
            Console.WriteLine("\n > Suggested ID: {0}\n", newID);

            do
            {
                Console.Write(" New ID (numeric): ");

                input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    newID = suggestedID;
                }
                else
                {
                    if (int.TryParse(input, out newID) && availableRecords.Contains(Record.GetEmptyRecord(newID)))
                    {
                        Console.WriteLine(" This ID already exists!");
                        input = "wrong input";
                    }
                }
            }
            while (!string.IsNullOrWhiteSpace(input) && !int.TryParse(input, out newID));

            newRecord = Record.GetEmptyRecord(newID);

            return RecordEditor.GetModifiedRecord(newRecord);
        }

        /// <summary>
        /// Returns a new unique ID for a new record.
        /// </summary>
        /// <returns>A new unique ID for a new record.</returns>
        private static int GetNewID()
        {
            if (availableRecords.Count > 0)
            {
                int newID = availableRecords[0].ID;

                for (int i = 0; i < availableRecords.Count; i++)
                {
                    if (availableRecords[i].ID > newID)
                    {
                        newID = availableRecords[i].ID;
                    }
                }

                return newID + 1;
            }

            return 0;
        }

        /// <summary>
        /// Draws all available records on the console and returns the selected record.
        /// </summary>
        /// <returns>The selected record.</returns>
        private static Record GetRecordFromSelection()
        {
            Console.WriteLine("\n There are currently {0} records available.\n", availableRecords.Count);

            if (availableRecords.Count > 0)
            {
                // List all available records.
                for (int i = 0; i < availableRecords.Count; i++)
                {
                    Console.WriteLine(" {0}. ID: {1}", i + 1, availableRecords[i].ID);
                }

                Console.WriteLine();

                int selectionID = -1;
                bool validInput = false;

                // The user has to enter a valid ID which exists.
                while (!validInput || !availableRecords.Contains(Record.GetEmptyRecord(selectionID)))
                {
                    Console.Write(" Your selection (numeric ID): ");

                    if ((validInput = int.TryParse(Console.ReadLine(), out selectionID)) && !availableRecords.Contains(Record.GetEmptyRecord(selectionID)))
                    {
                        Console.WriteLine(" This ID does not exist!");
                    }
                }

                // Return the selected record.
                for (int i = 0; i < availableRecords.Count; i++)
                {
                    if (availableRecords[i].ID == selectionID)
                    {
                        return availableRecords[i];
                    }
                }
            }
            else
            {
                Console.ReadLine();
            }

            return null;
        }

        /// <summary>
        /// Edits an existing record by the user.
        /// </summary>
        private static void EditRecord()
        {
            Console.Clear();
            Console.WriteLine("\n [F2] Edit record");

            Record r = GetRecordFromSelection();

            if (r != null)
            {
                RecordEditor.GetModifiedRecord(r);
            }
        }

        /// <summary>
        /// Deletes an existing record by the user.
        /// </summary>
        private static void DeleteRecord()
        {
            Console.Clear();
            Console.WriteLine("\n [F2] Delete record");

            Record r = GetRecordFromSelection();

            if (r != null)
            {
                if (availableRecords.Remove(r))
                {
                    Console.WriteLine(" The record has been deleted!");
                    Console.ReadLine();
                }
            }
        }

        /// <summary>
        /// Exports all existing records by the user.
        /// </summary>
        private static void ExportRecords()
        {
            Console.Clear();
            Console.WriteLine("\n [F4] Export records");

            Console.Write("\n Please enter a valid file name: ");

            string filename = Console.ReadLine();
            List<string> export = new List<string>();

            for (int i = 0; i < availableRecords.Count; i++)
            {
                export.Add(availableRecords[i].Export());
            }

            try
            {
                File.WriteAllLines(filename, export.ToArray());
                Console.WriteLine(" {0} records have been written to the file.", export.Count);
            }
            catch (IOException)
            {
                Console.WriteLine(" An error occurred while writing to the file!");
            }

            Console.ReadLine();
        }

        /// <summary>
        /// Imports all existing records by the user.
        /// </summary>
        private static void ImportRecords()
        {
            Console.Clear();
            Console.WriteLine("\n [F5] Import records");

            Console.Write("\n Please enter a valid file name: ");

            string filename = Console.ReadLine();
            int readRecords = 0;

            if (File.Exists(filename))
            {
                try
                {
                    string[] content = File.ReadAllLines(filename);

                    for (int i = 0; i < content.Length; i++)
                    {
                        Record r = Record.Parse(content[i]);

                        if (r != null)
                        {
                            if (!availableRecords.Contains(r))
                            {
                                availableRecords.Add(r);
                                readRecords++;
                            }
                            else
                            {
                                Console.WriteLine(" A record with the ID {0} already exists!", r.ID);
                            }
                        }
                    }

                    Console.WriteLine(" {0} records have been added.", readRecords);
                }
                catch (IOException)
                {
                    Console.WriteLine(" An error occurred while reading from the file!");
                }
            }
            else
            {
                Console.WriteLine(" The file does not exist!");
            }

            Console.ReadLine();
        }

        /// <summary>
        /// Prompts the user to enter an IP address and a port and sends all records to this server.
        /// </summary>
        private static void ConnectToServer()
        {
            Console.Clear();
            Console.WriteLine("\n [F6] Connect to server");

            if (availableRecords.Count == 0)
            {
                Console.WriteLine("\n There are no records available which could be sent over the network!");
            }
            else
            {
                Console.Write("\n Please enter a valid IP-address (for example 192.168.1.1): ");

                string address = Console.ReadLine();

                Console.Write("\n Please enter a valid port [0 - 65535] (Default: {0}): ", Protocol.DefaultServerPort);

                string port = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(port))
                {
                    port = Protocol.DefaultServerPort.ToString();
                }

                Console.WriteLine();

                // Only connect if address and port are valid.
                if (!RecordSender.IsValidIPAddress(address))
                {
                    Console.WriteLine(" The entered ip address is not valid!");
                }
                else if (!RecordSender.IsValidPort(port))
                {
                    Console.WriteLine(" The entered port is not valid!");
                }
                else
                {
                    RecordSender conn = new RecordSender(address, port);

                    conn.OnRecordAccepted += Conn_OnRecordAccepted;
                    conn.OnTransferFinished += Conn_OnTransferFinished;
                    conn.OnRecordRejectedDueToDuplicateIDs += Conn_OnRecordRejectedDueToDuplicateIDs;
                    conn.OnRecordRejectedDueToDuplicateTimestamps += Conn_OnRecordRejectedDueToDuplicateTimestamps;

                    // Connecting to server.
                    if (conn.Connect())
                    {
                        Console.WriteLine(" > Connection to {0}:{1} was successful.\n > Sending {2} records to the server....", address, port, availableRecords.Count);

                        conn.SendRecords(availableRecords);
                    }
                    else
                    {
                        Console.WriteLine(" > Connection to {0}:{1} could not be established!", address, port);
                    }
                }
            }

            Console.ReadLine();
        }

        /// <summary>
        /// Gets called when the record transfer has been finished.
        /// </summary>
        /// <param name="unconfirmedRecords">The amount of unconfirmed records.</param>
        /// <param name="acceptedRecords">The amount of accepted records.</param>
        /// <param name="rejectedRecords">The amount of rejected records.</param>
        private static void Conn_OnTransferFinished(int unconfirmedRecords, int acceptedRecords, int rejectedRecords)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("\n > ");
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.WriteLine("Transfer has been finished with following results:\n", acceptedRecords);
            Console.WriteLine("    {0, 4} records have been accepted.", acceptedRecords);
            Console.WriteLine("    {0, 4} records have been rejected.", rejectedRecords);
            Console.WriteLine("    {0, 4} records remained unconfirmed.", unconfirmedRecords);
        }

        /// <summary>
        /// Gets called when a record has been rejected by the server due to duplicate timestamps.
        /// </summary>
        /// <param name="record">The rejected record.</param>
        private static void Conn_OnRecordRejectedDueToDuplicateTimestamps(Record record)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("\n > ");
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.WriteLine("Record with the ID {0} has been rejected due to duplicate timestamps!", record.ID);
        }

        /// <summary>
        /// Gets called when a record has been rejected by the server due to duplicate IDs.
        /// </summary>
        /// <param name="record">The rejected record.</param>
        private static void Conn_OnRecordRejectedDueToDuplicateIDs(Record record)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("\n > ");
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.WriteLine("Record with the ID {0} has been rejected due to duplicate IDs!", record.ID);
        }

        /// <summary>
        /// Gets called when a record has been accepted by the server.
        /// </summary>
        /// <param name="record">The accepted record.</param>
        private static void Conn_OnRecordAccepted(Record record)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("\n > ");
            Console.ForegroundColor = ConsoleColor.Gray;

            Console.WriteLine("Record with the ID {0} has been accepted!", record.ID);
        }
    }
}
