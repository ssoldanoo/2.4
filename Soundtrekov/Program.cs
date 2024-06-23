using System.ComponentModel;
using Microsoft.EntityFrameworkCore;

namespace Soundtrecov;


public class program
{ 
    public partial class SoundtrecovContext : DbContext 
    { 
        static SoundtrecovContext() 
        { 
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true); 
        }

        public SoundtrecovContext()
        {
            
        }

        public SoundtrecovContext(DbContextOptions<SoundtrecovContext> options) : base(options)
        {
            
        }
        
        public virtual DbSet<Person> Persons { get; set; } 
        public virtual DbSet<Task> Tasks { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) 
        { 
            if (!optionsBuilder.IsConfigured) 
            { 
                optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=postgres;Username=postgres;Password="); 
            } 
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>(entity =>
            {
                entity.ToTable("person");
                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.login)
                    .HasMaxLength(50)
                    .HasColumnName("login");
                entity.Property(e => e.password)
                    .HasMaxLength(50)
                    .HasColumnName("password");
            });
    
            modelBuilder.Entity<Task>(entity =>
            {
                entity.ToTable("task");
                entity.Property(e => e.id).HasColumnName("id");
                entity.Property(e => e.personId).HasColumnName("personid");
                entity.Property(e => e.name).HasColumnName("name");
                entity.Property(e => e.description).HasColumnName("description");
                entity.Property(e => e.date).HasColumnName("date");
                
                entity.HasOne(t => t.idPersonNavigator).WithMany(p => p.Tasks)
                    .HasForeignKey(d => d.personId)
                    .HasConstraintName("task_personid_fkey");
            });
            OnModelCreatingPartial(modelBuilder);
        } 
        partial void OnModelCreatingPartial(ModelBuilder modelBuilder); 
    }
    
    public class Person
    {
        public int id { get; set; }
        public string login { get; set; }
        public string password { get; set; }
        public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
    }

    public class Task
    {
        public int id { get; set; }
        public int personId { get; set; }
        public string name { get; set; } 
        public string description { get; set; }
        public DateTime date { get; set; }
        public virtual Person idPersonNavigator { get; set; }
    }


    static void Main(string[] args)
    {
        int choice;
        Console.WriteLine("\n1 - Вход\n2 - Регистрация\n0 - Выход");
        choice = int.Parse(Console.ReadLine());

        if (choice == 1)
        {
            using (SoundtrecovContext db = new SoundtrecovContext())
            {
                var persons = db.Persons.ToList();
                for (int i = 0; i < persons.Count; i++)
                {
                    Console.WriteLine($"{i + 1}: {persons[i].login}");
                }

                int choicePerson = int.Parse(Console.ReadLine());

                if (choicePerson >= 1)
                {
                    var person = persons[choicePerson - 1];
                    Console.WriteLine($"\nВведите пароль: ");
                    string password = Console.ReadLine();

                    if (person.password == password)
                    {
                        taskSelect(person.id);
                    }
                    else
                    {
                        Console.WriteLine("Неверный пароль!");
                    }
                }
                else
                {
                    persons = db.Persons.ToList();
                    Console.Write("\nВведите логин: ");
                    string login = Console.ReadLine();
                    Console.Write("Введите пароль: ");
                    string password = Console.ReadLine();

                    Person newPerson = new Person()
                    {
                        id = persons.Count + 1,
                        login = login,
                        password = password
                    };

                    db.Persons.Add(newPerson);
                    db.SaveChanges();
                    Console.WriteLine("\nРегистрация завершена!");
                }
            }
        }
        else if (choice == 2)
        {
            SoundtrecovContext db = new SoundtrecovContext();
            var persons = db.Persons.ToList();
            Console.Write("\nВведите логин: ");
            string login = Console.ReadLine();
            Console.Write("Введите пароль: ");
            string password = Console.ReadLine();

            Person newPerson = new Person()
            {
                id = persons.Count + 1,
                login = login,
                password = password
            };

            db.Persons.Add(newPerson);
            db.SaveChanges();
            Console.WriteLine("\nРегистрация завершена!");
        }
    }

    static void taskSelect(int personId)
    {
        Console.WriteLine(
            "\n1 - Добавить задачу\n2 - Удалить задачу\n3 - Редактировать задачу\n4 - Просмотр задач\n5 - Сменить пользователя");
        int select = int.Parse(Console.ReadLine());

        switch (select)
        {
            case 1:
                SoundtrecovContext datebase = new SoundtrecovContext();
                Console.Write("\nНазвание: ");
                string nameTask = Console.ReadLine();
                Console.Write("Описание: ");
                string descriptionTask = Console.ReadLine();
                Console.Write("Дата: ");
                DateTime dateTask = DateTime.Parse(Console.ReadLine());

                Task newTask = new Task()
                {
                    personId = personId,
                    name = nameTask,
                    description = descriptionTask,
                    date = dateTask
                };

                datebase.Tasks.Add(newTask);
                datebase.SaveChanges();
                Console.Write($"\nЗадача добавлена!\n");
                taskSelect(personId);
                break;
            case 2: 
                datebase = new SoundtrecovContext();
                Console.WriteLine();
                var taskList = datebase.Tasks.Include(r => r.idPersonNavigator).Where(r => r.idPersonNavigator.id == personId)
                    .ToList();

                foreach (var n in taskList)
                {
                    Console.WriteLine($"{n.id}: {n.name}");
                }

                Console.Write("\nId задачи для удаления: ");
                int choiceTask = int.Parse(Console.ReadLine());
                var taskRemove = taskList.FirstOrDefault(t => t.id == choiceTask);
                Console.Write($"\nЗадача удалена\n");
                datebase.Tasks.Remove(taskRemove);
                datebase.SaveChanges();
                taskSelect(personId);
                break;
            case 3: 
                datebase = new SoundtrecovContext();
                Console.WriteLine();
                taskList = datebase.Tasks.Include(r => r.idPersonNavigator).Where(r => r.idPersonNavigator.id == personId)
                    .ToList();

                foreach (var n in taskList)
                {
                    Console.WriteLine($"{n.id}: {n.name}");
                }

                Console.Write("\nId задачи для редактирования: ");
                choiceTask = int.Parse(Console.ReadLine());

                Console.Write("\nНазвание: ");
                string name = Console.ReadLine();
                Console.Write("Описание: ");
                string description = Console.ReadLine();
                Console.Write("Дата: ");
                DateTime date = DateTime.Parse(Console.ReadLine());
                var updateTask = taskList.FirstOrDefault(t => t.id == choiceTask);
                updateTask.name = name;
                updateTask.description = description;
                updateTask.date = date;
                datebase.SaveChanges();
                Console.Write("\nЗадача обновлена\n");
                taskSelect(personId);
                break;
            case 4:
                datebase = new SoundtrecovContext();
                Console.WriteLine(
                    "\n1 - на сегодня\n2 - на завтра\n3 - на неделю\n4 - выполнено\n5 - предстоящие\n6 - все задачи");
                int choice = int.Parse(Console.ReadLine());

                switch (choice)
                {
                    case 1:
                        foreach (var task in datebase.Tasks.Include(r => r.idPersonNavigator).Where(r =>
                                     r.idPersonNavigator.id == personId && r.date.Date == DateTime.Today))
                        {
                            Console.WriteLine(
                                $"\nНазвание: {task.name}\nОписание: {task.description}\nДата: {task.date}");
                        }

                        Console.WriteLine();
                        taskSelect(personId);
                        break;
                    case 2:
                        foreach (var task in datebase.Tasks.Include(r => r.idPersonNavigator).Where(r =>
                                     r.idPersonNavigator.id == personId && r.date.Date == DateTime.Today.AddDays(1)))
                        {
                            Console.WriteLine(
                                $"\nНазвание: {task.name}\nОписание: {task.description}\nДата: {task.date}");
                        }

                        Console.WriteLine();
                        taskSelect(personId);
                        break;
                    case 3:
                        foreach (var task in datebase.Tasks.Include(r => r.idPersonNavigator).Where(r =>
                                     r.idPersonNavigator.id == personId && r.date.Date >= DateTime.Today &&
                                     r.date.Date <= DateTime.Today.AddDays(7)))
                        {
                            Console.WriteLine(
                                $"\nНазвание: {task.name}\nОписание: {task.description}\nДата: {task.date}");
                        }

                        Console.WriteLine();
                        taskSelect(personId);
                        break;
                    case 4:
                        foreach (var task in datebase.Tasks.Include(r => r.idPersonNavigator).Where(r =>
                                     r.idPersonNavigator.id == personId && r.date.Date < DateTime.Today))
                        {
                            Console.WriteLine(
                                $"\nЗадача: {task.name}\nОписание: {task.description}\nДата: {task.date}");
                        }

                        Console.WriteLine();
                        taskSelect(personId);
                        break;
                    case 5:
                        foreach (var task in datebase.Tasks.Include(r => r.idPersonNavigator).Where(r =>
                                     r.idPersonNavigator.id == personId && r.date.Date > DateTime.Today))
                        {
                            Console.WriteLine(
                                $"\nЗадача: {task.name}\nОписание: {task.description}\nДата: {task.date}");
                        }

                        Console.WriteLine();
                        taskSelect(personId);
                        break;
                    case 6:
                        foreach (var task in datebase.Tasks.Include(r => r.idPersonNavigator)
                                     .Where(r => r.idPersonNavigator.id == personId))
                        {
                            Console.WriteLine(
                                $"\nЗадача: {task.name}\nОписание: {task.description}\nДата: {task.date}");
                        }

                        Console.WriteLine();
                        taskSelect(personId);
                        break;
                }
                break;
        }
    }
}