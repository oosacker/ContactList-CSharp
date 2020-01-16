using CsvHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContactList
{
    class Program
    {
        static Dictionary<string, Contact> Contacts = new Dictionary<string, Contact>();

        static void Main()
        {
            UserInterface();
        }

        static void UserInterface()
        {
            while (true)
            {
                Console.WriteLine("\nCommand list:");
                Console.WriteLine("print: Print all contacts to screen");
                Console.WriteLine("add: Add new contact");
                Console.WriteLine("edit: Edit existing contact");
                Console.WriteLine("find: Find contact by name");
                Console.WriteLine("save: Save contact list to CSV");
                Console.WriteLine("load: Load contact list from CSV");
                Console.WriteLine("delete: Delete a contact");
                Console.WriteLine("delete-all: Delete all contacts");
                Console.WriteLine("quit: Quit program");

                Console.WriteLine("\nCommand? ");
                string line = Console.ReadLine().Trim().ToLower();
                switch (line)
                {
                    case "print":
                        {
                            PrintContacts();
                            break;
                        }

                    case "add":
                        {
                            AddContact();
                            break;
                        }

                    case "edit":
                        {
                            EditContact();
                            break;
                        }

                    case "find":
                        {
                            FindContact();
                            break;
                        }

                    case "save":
                        {
                            SaveContacts();
                            break;
                        }

                    case "load":
                        {
                            LoadContacts();
                            break;
                        }

                    case "delete":
                        {
                            DeleteContact();
                            break;
                        }

                    case "delete-all":
                        {
                            DeleteAllContacts();
                            break;
                        }

                    case "quit":
                        {
                            Environment.Exit(0);
                            break;
                        }

                    default:
                        {
                            Console.WriteLine("Invalid command\n");
                            break;
                        }
                }
            }
        }

        static void PrintContacts()
        {
            if (Contacts.Count != 0)
            {
                foreach (KeyValuePair<string, Contact> Pair in Contacts)
                {
                    Console.WriteLine("\n" + Pair.Value.ToString());
                }
            }
            else
            {
                Console.WriteLine("Contact list is empty\n");
                return;
            }

        }

        static void AddContact()
        {
            Console.WriteLine("Name? ");
            string name_str = Console.ReadLine().Trim();

            if (Contacts.ContainsKey(name_str))
            {
                Console.WriteLine("Contact name \"{0}\" already in list\n", name_str);
                return;
            }

            // Using a long integer (64bit) otherwise can be invalid
            Console.WriteLine("Phone? ");
            string phone_str = Console.ReadLine().Trim();
            if(!Int64.TryParse(phone_str, out long phone_num))
            {
                Console.WriteLine("Invalid phone number");
                return;
            }

            Console.WriteLine("Email? ");
            string email_str = Console.ReadLine().Trim();

            Console.WriteLine("Occupation? ");
            string occu_str = Console.ReadLine().Trim();

            Contact con = new Contact(name_str, phone_num, email_str, occu_str);
            Contacts.Add(name_str, con);

            Console.WriteLine(con.ToString());
        }

        static void EditContact()
        {
            if (Contacts.Count != 0)
            {
                Contact contact_edit = FindContact();
                if(contact_edit == null)
                {
                    return;
                }
                else
                {
                    while (true){
                        Console.WriteLine("Edit what? ");
                        string line = Console.ReadLine().Trim().ToLower();
                        switch (line)
                        {
                            case "name":
                                {
                                    // Backup the old name
                                    string old_name = contact_edit.Name;

                                    // Read new name
                                    Console.WriteLine("New name? ");
                                    string new_name = Console.ReadLine().Trim();

                                    // Update the dictionary entry
                                    contact_edit.Name = new_name;
                                    Contacts[new_name] = contact_edit;
                                    Contacts.Remove(old_name);

                                    break;
                                }
                            case "phone":
                                {
                                    Console.WriteLine("New phone? ");
                                    string phone_str = Console.ReadLine().Trim();
                                    if (!Int64.TryParse(phone_str, out long phone_num))
                                    {
                                        Console.WriteLine("Invalid phone number");
                                        return;
                                    }
                                    contact_edit.Phone = phone_num;
                                    break;
                                }
                            case "email":
                                {
                                    Console.WriteLine("New email? ");
                                    string email_str = Console.ReadLine().Trim();
                                    contact_edit.Email = email_str;
                                    break;
                                }
                            case "occupation":
                                {
                                    Console.WriteLine("New occupation? ");
                                    string occ_str = Console.ReadLine().Trim();
                                    contact_edit.Occupation = occ_str;
                                    break;
                                }
                            default:
                                Console.WriteLine("Invalid input\n");
                                break;
                        }
                        Console.WriteLine("\n" + contact_edit.ToString());
                        return;     // Otherwise it will keep looping
                    }

                }

            }
            else
            {
                Console.WriteLine("Contact list is empty\n");
                return;
            }
        }

        static Contact FindContact()
        {
            if (Contacts.Count != 0)
            {
                Console.WriteLine("Name of contact? ");
                string input_str = Console.ReadLine().Trim();
                if (Contacts.ContainsKey(input_str))
                {
                    Contact contact_found = Contacts[input_str];
                    Console.WriteLine(contact_found.ToString());
                    return contact_found;
                }
                else
                {
                    Console.WriteLine("Contact not found\n");
                    return null;
                }
            }
            Console.WriteLine("Contact list is empty\n");
            return null;
        }

        static void SaveContacts()
        {
            if (Contacts.Count != 0)
            {
                Console.WriteLine("Name of file to save? ");
                string filename = Console.ReadLine().Trim();

                try
                {
                    // Convert the dictionary into a list so it can be processed by CsvWriter
                    List<Contact> MyContactList = Contacts.Values.ToList<Contact>();

                    using (var writer = new StreamWriter(filename))
                    using (var csv = new CsvWriter(writer))
                    {
                        csv.Configuration.SanitizeForInjection = false; // Prevent injection attack
                        csv.Configuration.HasHeaderRecord = false;      // No header line
                        csv.WriteRecords(MyContactList);
                    }
                }
                catch (IOException)
                {
                    Console.WriteLine("Saving file failed\n");
                }
                finally
                {
                    Console.WriteLine("\"{0}\" has been saved\n", filename);
                }
            }
            else
            {
                Console.WriteLine("Contact list is empty\n");
                return;
            }
        }

        static void LoadContacts()
        {
            Console.WriteLine("Name of file to load? ");
            string filename = Console.ReadLine().Trim();

            try
            {
                // Note that CsvReader deals with lists not dictionaries!!! 
                using (var reader = new StreamReader(filename))
                using (var csv = new CsvReader(reader))
                {
                    // IEnumerable is an INTERFACE not a class!!!!!!
                    IEnumerable<Contact> NewContacts = new List<Contact>();

                    // No header row in csv
                    csv.Configuration.HasHeaderRecord = false;

                    // Get data from csv file into list
                    NewContacts = csv.GetRecords<Contact>();

                    // Add the new contacts to the dictionary
                    List<Contact> NewContactsList = NewContacts.ToList();
                    int load_count = 0;
                    foreach (Contact con in NewContactsList)
                    {
                        // Must skip duplicates or will crash!!!!
                        if (Contacts.ContainsKey(con.Name))
                        {
                            Console.WriteLine("Contact \"{0}\" already in list; skipping", con.Name);
                        }
                        else
                        {
                            Contacts.Add(con.Name, con);
                            load_count++;
                        }
                    }
                    Console.WriteLine("Number of contacts loaded: " + load_count);
                }
            }
            catch(FileNotFoundException)
            {
                Console.WriteLine("File not found\n");
            }
        }

        static void DeleteContact()
        {
            if (Contacts.Count != 0)
            {
                Contact contact_found = FindContact();
                if(contact_found != null)
                {
                    string name = contact_found.Name;
                    Contacts.Remove(name);
                    Console.WriteLine("\"{0}\" was deleted\n", name);
                }
            }
            else
            {
                Console.WriteLine("Contact list is empty\n");
                return;
            }
        }

        static void DeleteAllContacts()
        {
            if (Contacts.Count != 0)
            {
                Contacts.Clear();
                Console.WriteLine("All contacts deleted\n");
            }
            else
            {
                Console.WriteLine("Contact list is empty\n");
                return;
            }
        }

    }

    class Contact
    {
        // Constructor
        public Contact(string name = "name", long phone = 0, string email = "@", string occupation = "occupation")
        {
            Name = name;
            Phone = phone;
            Email = email;
            Occupation = occupation;
        }

        // Using autoproperties for fields
        public string Name
        {
            set; get;
        }

        public long Phone
        {
            set; get;
        }

        public string Email
        {
            set; get;
        }

        public string Occupation
        {
            set; get;
        }

        // Overriding the built-in ToString method
        public override string ToString()
        {
            return "Name: " + Name + "\nPhone: " + Phone + "\nEmail: " + Email + "\nOccupation: " + Occupation + "\n";
        }
    }
}
