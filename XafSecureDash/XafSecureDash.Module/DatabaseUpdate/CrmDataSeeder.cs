using DevExpress.ExpressApp;
using XafSecureDash.Module.BusinessObjects.Crm;

namespace XafSecureDash.Module.DatabaseUpdate
{
    public static class CrmDataSeeder
    {
        private static readonly Random Rng = new(42); // Fixed seed for reproducibility

        private static readonly string[] Industries = { "Technology", "Healthcare", "Finance", "Manufacturing", "Retail", "Energy", "Consulting", "Education", "Real Estate", "Logistics" };
        private static readonly string[] Cities = { "Amsterdam", "Rotterdam", "Utrecht", "The Hague", "Eindhoven", "Groningen", "Breda", "Tilburg", "Maastricht", "Leiden", "Brussels", "Antwerp", "Ghent", "London", "Paris", "Berlin", "Munich", "Frankfurt", "Zurich", "Vienna" };
        private static readonly string[] Countries = { "Netherlands", "Netherlands", "Netherlands", "Netherlands", "Netherlands", "Netherlands", "Netherlands", "Netherlands", "Netherlands", "Netherlands", "Belgium", "Belgium", "Belgium", "UK", "France", "Germany", "Germany", "Germany", "Switzerland", "Austria" };
        private static readonly string[] FirstNames = { "Jan", "Pieter", "Klaas", "Anna", "Sophie", "Emma", "Daan", "Lucas", "Liam", "Julia", "Sara", "Thomas", "Max", "Lars", "Eva", "Lisa", "Mark", "Peter", "Maria", "Carmen", "Hans", "Frank", "Claire", "Nina", "Wouter" };
        private static readonly string[] LastNames = { "de Vries", "Jansen", "van Dijk", "Bakker", "Visser", "Smit", "Meijer", "de Boer", "Mulder", "de Groot", "Bos", "Peters", "Hendriks", "van Leeuwen", "Dekker", "Brouwer", "de Wit", "Dijkstra", "Vermeer", "van den Berg" };
        private static readonly string[] JobTitles = { "CEO", "CTO", "CFO", "VP Sales", "VP Engineering", "Director", "Manager", "Senior Consultant", "Analyst", "Developer", "Architect", "Project Manager", "Account Manager", "Business Analyst", "Operations Manager" };
        private static readonly string[] Departments = { "Management", "Engineering", "Sales", "Marketing", "Finance", "HR", "Operations", "IT", "Legal", "Support" };
        private static readonly string[] ProductCategories = { "Software License", "Consulting Hours", "Training", "Support Contract", "Cloud Service", "Hardware", "Custom Development", "Integration Service" };
        private static readonly string[] ProjectTypes = { "Digital Transformation", "ERP Implementation", "Cloud Migration", "Data Analytics", "Security Audit", "Process Optimization", "System Integration", "Custom Development", "IT Strategy", "Training Program" };

        public static void Seed(IObjectSpace os)
        {
            if (os.GetObjects<Company>().Any())
                return;

            var companies = SeedCompanies(os);
            var contacts = SeedContacts(os, companies);
            var products = SeedProducts(os);
            os.CommitChanges();

            var orders = SeedOrders(os, companies, contacts, products);
            os.CommitChanges();

            SeedInvoices(os, companies, orders, products);
            os.CommitChanges();

            var projects = SeedProjects(os, companies, contacts);
            os.CommitChanges();

            SeedTimeEntries(os, projects, contacts);
            os.CommitChanges();
        }

        private static List<Company> SeedCompanies(IObjectSpace os)
        {
            var companyNames = new[]
            {
                "TechVista Solutions", "MedCore Systems", "FinBridge Capital", "AutoWerk Industries",
                "ShopStream Retail", "GreenPower Energy", "Apex Consulting Group", "EduTech Learning",
                "PrimeVest Properties", "SwiftLog Transport", "CloudNine Software", "BioHealth Labs",
                "Quantum Finance", "SteelForge Manufacturing", "MarketPulse Analytics",
                "NovaTech Innovations", "GlobalLink Logistics", "DataDrive Solutions",
                "PeakView Consulting", "SmartBuild Construction", "Nexus Communications",
                "AlphaWave Technologies", "ClearPath Systems", "BlueSky Ventures",
                "IronGate Security", "GreenLeaf Organics", "TrueNorth Analytics",
                "RapidScale Cloud", "CoreLogic Partners", "Vanguard Engineering",
                "BrightStar Media", "OceanView Hospitality", "PrecisionMed Devices",
                "AgileWorks Consulting", "SilverLine Banking", "FrostByte Gaming",
                "Harmony Healthcare", "TerraForm Energy", "PixelCraft Studios",
                "MetroConnect Telecom", "SafeHaven Insurance", "LightSpeed Fiber",
                "AquaPure Systems", "EliteForce Training", "UrbanEdge Development",
                "DigiMark Advertising", "HorizonAir Drones", "CrystalClear Optics",
                "TimberWolf Forestry", "PolarStar Navigation"
            };

            var companies = new List<Company>();
            for (int i = 0; i < companyNames.Length; i++)
            {
                var c = os.CreateObject<Company>();
                c.Name = companyNames[i];
                c.Industry = Industries[i % Industries.Length];
                c.City = Cities[i % Cities.Length];
                c.Country = Countries[i % Countries.Length];
                c.Address = $"{Rng.Next(1, 500)} {LastNames[i % LastNames.Length]}straat";
                c.Phone = $"+31 {Rng.Next(10, 99)} {Rng.Next(100, 999)} {Rng.Next(1000, 9999)}";
                c.Website = $"https://www.{companyNames[i].ToLower().Replace(" ", "").Replace(".", "")}.com";
                c.EmployeeCount = Rng.Next(5, 5000);
                c.AnnualRevenue = Rng.Next(100_000, 50_000_000);
                c.CreatedDate = DateTime.Today.AddDays(-Rng.Next(30, 1500));
                companies.Add(c);
            }
            return companies;
        }

        private static List<Contact> SeedContacts(IObjectSpace os, List<Company> companies)
        {
            var contacts = new List<Contact>();
            foreach (var company in companies)
            {
                int contactCount = Rng.Next(2, 8);
                for (int j = 0; j < contactCount; j++)
                {
                    var ct = os.CreateObject<Contact>();
                    ct.FirstName = FirstNames[Rng.Next(FirstNames.Length)];
                    ct.LastName = LastNames[Rng.Next(LastNames.Length)];
                    ct.Email = $"{ct.FirstName.ToLower()}.{ct.LastName.ToLower().Replace(" ", "")}@{company.Name.ToLower().Replace(" ", "").Replace(".", "")}.com";
                    ct.Phone = $"+31 6 {Rng.Next(10000000, 99999999)}";
                    ct.JobTitle = JobTitles[Rng.Next(JobTitles.Length)];
                    ct.Department = Departments[Rng.Next(Departments.Length)];
                    ct.Company = company;
                    ct.CreatedDate = company.CreatedDate.AddDays(Rng.Next(0, 60));
                    ct.IsActive = Rng.NextDouble() > 0.1;
                    contacts.Add(ct);
                }
            }
            return contacts;
        }

        private static List<Product> SeedProducts(IObjectSpace os)
        {
            var productDefs = new (string Name, string Category, decimal Price, decimal Cost)[]
            {
                ("XAF Enterprise License", "Software License", 4999m, 1000m),
                ("XAF Standard License", "Software License", 1999m, 400m),
                ("Blazor Starter Pack", "Software License", 799m, 150m),
                ("Senior Consultant - Day Rate", "Consulting Hours", 1600m, 800m),
                ("Consultant - Day Rate", "Consulting Hours", 1200m, 600m),
                ("Junior Consultant - Day Rate", "Consulting Hours", 800m, 400m),
                ("XAF Fundamentals Training (3d)", "Training", 3600m, 1200m),
                ("Advanced Blazor Workshop (2d)", "Training", 2800m, 900m),
                ("Security Best Practices (1d)", "Training", 1500m, 500m),
                ("Premium Support (Annual)", "Support Contract", 12000m, 3000m),
                ("Standard Support (Annual)", "Support Contract", 6000m, 1500m),
                ("Basic Support (Annual)", "Support Contract", 2400m, 600m),
                ("Cloud Hosting - Enterprise", "Cloud Service", 2400m, 800m),
                ("Cloud Hosting - Standard", "Cloud Service", 1200m, 400m),
                ("Cloud Hosting - Starter", "Cloud Service", 480m, 160m),
                ("Server Hardware Bundle", "Hardware", 8500m, 5500m),
                ("Workstation Setup", "Hardware", 2200m, 1400m),
                ("Network Equipment Package", "Hardware", 3800m, 2400m),
                ("Custom Module Development", "Custom Development", 15000m, 7500m),
                ("API Integration Package", "Integration Service", 8000m, 4000m),
                ("Data Migration Service", "Integration Service", 5000m, 2500m),
                ("Report Builder Add-on", "Software License", 599m, 120m),
                ("Dashboard Designer Pro", "Software License", 1299m, 260m),
                ("Mobile Access Module", "Software License", 999m, 200m),
                ("DevOps Pipeline Setup", "Custom Development", 6000m, 3000m),
            };

            var products = new List<Product>();
            foreach (var (name, category, price, cost) in productDefs)
            {
                var p = os.CreateObject<Product>();
                p.Name = name;
                p.Category = category;
                p.UnitPrice = price;
                p.Cost = cost;
                p.SKU = $"PRD-{products.Count + 1:D4}";
                p.StockQuantity = category == "Hardware" ? Rng.Next(5, 100) : 9999;
                p.Description = $"{name} - {category}";
                p.IsActive = true;
                p.CreatedDate = DateTime.Today.AddDays(-Rng.Next(100, 800));
                products.Add(p);
            }
            return products;
        }

        private static List<Order> SeedOrders(IObjectSpace os, List<Company> companies, List<Contact> contacts, List<Product> products)
        {
            var orders = new List<Order>();
            int orderNum = 1000;

            foreach (var company in companies)
            {
                int orderCount = Rng.Next(1, 12);
                var companyContacts = contacts.Where(c => c.Company == company).ToList();

                for (int i = 0; i < orderCount; i++)
                {
                    var order = os.CreateObject<Order>();
                    order.OrderNumber = $"ORD-{orderNum++}";
                    order.Company = company;
                    order.Contact = companyContacts.Any() ? companyContacts[Rng.Next(companyContacts.Count)] : null;
                    order.OrderDate = DateTime.Today.AddDays(-Rng.Next(1, 730));
                    order.Status = (OrderStatus)Rng.Next(0, 6);
                    if (order.Status >= OrderStatus.Shipped)
                        order.ShippedDate = order.OrderDate.AddDays(Rng.Next(1, 14));

                    int lineCount = Rng.Next(1, 6);
                    decimal total = 0;
                    for (int j = 0; j < lineCount; j++)
                    {
                        var line = os.CreateObject<OrderLine>();
                        line.Order = order;
                        line.Product = products[Rng.Next(products.Count)];
                        line.Quantity = Rng.Next(1, 20);
                        line.UnitPrice = line.Product.UnitPrice;
                        line.LineTotal = line.Quantity * line.UnitPrice;
                        total += line.LineTotal;
                    }

                    order.Discount = Rng.NextDouble() > 0.7 ? Math.Round(total * (decimal)(Rng.NextDouble() * 0.15), 2) : 0;
                    order.TotalAmount = total - order.Discount;
                    order.Notes = Rng.NextDouble() > 0.5 ? $"Priority: {(Rng.NextDouble() > 0.5 ? "High" : "Normal")}" : null;
                    orders.Add(order);
                }
            }
            return orders;
        }

        private static void SeedInvoices(IObjectSpace os, List<Company> companies, List<Order> orders, List<Product> products)
        {
            int invNum = 2000;
            var deliveredOrders = orders.Where(o => o.Status >= OrderStatus.Delivered).ToList();

            foreach (var order in deliveredOrders)
            {
                var inv = os.CreateObject<Invoice>();
                inv.InvoiceNumber = $"INV-{invNum++}";
                inv.Company = order.Company;
                inv.Order = order;
                inv.InvoiceDate = (order.ShippedDate ?? order.OrderDate).AddDays(Rng.Next(1, 7));
                inv.DueDate = inv.InvoiceDate.AddDays(30);
                inv.TaxAmount = Math.Round(order.TotalAmount * 0.21m, 2); // 21% VAT
                inv.TotalAmount = order.TotalAmount + inv.TaxAmount;

                double roll = Rng.NextDouble();
                if (roll < 0.6)
                {
                    inv.Status = InvoiceStatus.Paid;
                    inv.PaidDate = inv.InvoiceDate.AddDays(Rng.Next(5, 45));
                }
                else if (roll < 0.8)
                    inv.Status = InvoiceStatus.Sent;
                else if (roll < 0.95)
                    inv.Status = InvoiceStatus.Overdue;
                else
                    inv.Status = InvoiceStatus.Draft;

                // Copy order lines to invoice lines
                foreach (var ol in order.Lines)
                {
                    var il = os.CreateObject<InvoiceLine>();
                    il.Invoice = inv;
                    il.Product = ol.Product;
                    il.Description = ol.Product?.Name ?? "Service";
                    il.Quantity = ol.Quantity;
                    il.UnitPrice = ol.UnitPrice;
                    il.LineTotal = ol.LineTotal;
                }
            }

            // Some standalone invoices (for recurring services)
            for (int i = 0; i < 30; i++)
            {
                var company = companies[Rng.Next(companies.Count)];
                var inv = os.CreateObject<Invoice>();
                inv.InvoiceNumber = $"INV-{invNum++}";
                inv.Company = company;
                inv.InvoiceDate = DateTime.Today.AddDays(-Rng.Next(1, 365));
                inv.DueDate = inv.InvoiceDate.AddDays(30);

                var product = products.Where(p => p.Category == "Support Contract" || p.Category == "Cloud Service").ToList();
                var selectedProduct = product[Rng.Next(product.Count)];
                decimal amount = selectedProduct.UnitPrice;
                inv.TaxAmount = Math.Round(amount * 0.21m, 2);
                inv.TotalAmount = amount + inv.TaxAmount;
                inv.Status = Rng.NextDouble() < 0.7 ? InvoiceStatus.Paid : InvoiceStatus.Sent;
                if (inv.Status == InvoiceStatus.Paid)
                    inv.PaidDate = inv.InvoiceDate.AddDays(Rng.Next(5, 30));

                var il = os.CreateObject<InvoiceLine>();
                il.Invoice = inv;
                il.Product = selectedProduct;
                il.Description = $"{selectedProduct.Name} - Monthly";
                il.Quantity = 1;
                il.UnitPrice = selectedProduct.UnitPrice;
                il.LineTotal = selectedProduct.UnitPrice;
            }
        }

        private static List<ConsultancyProject> SeedProjects(IObjectSpace os, List<Company> companies, List<Contact> contacts)
        {
            var projects = new List<ConsultancyProject>();
            int projNum = 100;

            // ~40 projects across companies
            for (int i = 0; i < 40; i++)
            {
                var company = companies[Rng.Next(companies.Count)];
                var companyContacts = contacts.Where(c => c.Company == company).ToList();
                var projectType = ProjectTypes[Rng.Next(ProjectTypes.Length)];

                var proj = os.CreateObject<ConsultancyProject>();
                proj.Name = $"{projectType} - {company.Name}";
                proj.ProjectCode = $"PRJ-{projNum++}";
                proj.Company = company;
                proj.PrimaryContact = companyContacts.Any() ? companyContacts[Rng.Next(companyContacts.Count)] : null;
                proj.StartDate = DateTime.Today.AddDays(-Rng.Next(30, 600));
                proj.Status = (ProjectStatus)Rng.Next(0, 5);
                if (proj.Status == ProjectStatus.Completed)
                    proj.EndDate = proj.StartDate.AddDays(Rng.Next(30, 180));
                proj.HourlyRate = new[] { 120m, 150m, 175m, 200m, 225m }[Rng.Next(5)];
                proj.EstimatedHours = Rng.Next(40, 800);
                proj.BudgetAmount = proj.HourlyRate * proj.EstimatedHours;
                proj.Description = $"{projectType} engagement for {company.Name}";
                projects.Add(proj);
            }
            return projects;
        }

        private static void SeedTimeEntries(IObjectSpace os, List<ConsultancyProject> projects, List<Contact> contacts)
        {
            var consultants = contacts.Where(c =>
                c.JobTitle != null && (c.JobTitle.Contains("Consultant") || c.JobTitle.Contains("Developer") || c.JobTitle.Contains("Architect") || c.JobTitle.Contains("Analyst")))
                .Take(20).ToList();

            if (!consultants.Any()) consultants = contacts.Take(10).ToList();

            foreach (var project in projects.Where(p => p.Status == ProjectStatus.Active || p.Status == ProjectStatus.Completed))
            {
                int entryCount = Rng.Next(10, 60);
                for (int i = 0; i < entryCount; i++)
                {
                    var te = os.CreateObject<TimeEntry>();
                    te.Project = project;
                    te.Consultant = consultants[Rng.Next(consultants.Count)];
                    te.Date = project.StartDate.AddDays(Rng.Next(0, (int)Math.Min((DateTime.Today - project.StartDate).TotalDays, 365)));
                    te.Hours = new[] { 2m, 4m, 6m, 7.5m, 8m }[Rng.Next(5)];
                    te.IsBillable = Rng.NextDouble() > 0.15;
                    te.IsInvoiced = te.IsBillable && Rng.NextDouble() > 0.3;
                    te.Description = GenerateTimeEntryDescription(Rng);
                }
            }
        }

        private static string GenerateTimeEntryDescription(Random rng)
        {
            var activities = new[]
            {
                "Requirements analysis meeting", "Sprint planning session", "Code review",
                "Architecture design", "Database schema design", "API development",
                "Frontend development", "Unit testing", "Integration testing",
                "Bug fixing", "Performance optimization", "Documentation",
                "Client workshop", "Stakeholder presentation", "Deployment preparation",
                "Security review", "Code refactoring", "Data migration",
                "Training session", "Technical support", "Infrastructure setup",
                "DevOps configuration", "Monitoring setup", "Release management"
            };
            return activities[rng.Next(activities.Length)];
        }
    }
}
