using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ЛР_5
{// РАСПРЕДЕЛЕНИЕ ПАМЯТИ РАЗДЕЛАМИ ПЕРЕМЕННОЙ ВЕЛИЧИНЫ
    class Program
    {
        static int COUNT = 0;
        public const int MEMORY_SIZE = 64000;
        static Queue<int> MEMORY = new Queue<int>();
        static Queue<int> QUEUE = new Queue<int>();
        static Object locker = new Object();
        //  ЗАДАЕМ РАЗМЕР ЗАДАЧИ 
        static int Random()
        {
            Random rand = new Random();
            return rand.Next(1, 65535);
        }
        //  ИМИТИРУЕМ РАБОТУ
        static void ProccesDo()
        {
            Random rand = new Random();
            Console.WriteLine("Задача делает работу");
            Thread.Sleep(rand.Next(2000, 5000));
            if (MEMORY.Count != 0)
            {
                MEMORY.Dequeue();   // удаляем первого в очереди, тк отработал свое
                Console.WriteLine("Задача удалена");
            }       
        }
        static void QueueDo()
        {
            int count = 0;
            if (MEMORY.Count == 0 && QUEUE.Count == 0)
                Console.WriteLine("НЕТ ЗАДАЧ !");
            foreach (int m in MEMORY)
            {
                count += m;
            }
            if (QUEUE.Count != 0)   //  если в очереди есть элементы, то
            {
                if ((count + QUEUE.Peek()) <= MEMORY_SIZE)  // проверяем хватит ли место в памяти записать первый элемент из очереди
                {
                    MEMORY.Enqueue(QUEUE.Dequeue());    //  если хватило, то добавляем в память и убираем из очереди,
                }                                       // если нет, то выходим
            }
        }
        static void CreateFile(string name, int size)
        {
            using (FileStream fWrite = new FileStream(name, FileMode.Create, FileAccess.Write)) //  создаем файл определенного размера
                fWrite.SetLength(size);
            COUNT++;
        }
        static void MemoryInfo()
        {
            int occupiedMemory = 0;
            foreach (int m in MEMORY)
            {
                occupiedMemory += m;
            }
            Console.WriteLine($"Задач: {MEMORY.Count} | Свободно: {MEMORY_SIZE - occupiedMemory} | Занято: {occupiedMemory}");
        }
        static void QueueInfo()
        {
            Console.WriteLine($"Задач в очереди: {QUEUE.Count}");
        }
        static void ProccesAdd()
        {
            int sizeOfFile, count = 0;
            string fileName = $"C:\\Users\\1255970\\Desktop\\OS\\ЛР_5\\ГЛАВНАЯ\\{COUNT + 1}.txt";    
           // StreamWriter fileWrite = new StreamWriter(fileName);
           // FileInfo fileInfo = new FileInfo(fileName);
            sizeOfFile = Random();
            CreateFile(fileName, sizeOfFile);
            foreach (int m in MEMORY)   //  высчитывам количество занятого пространства (ЗП)
                count += m;
            if ((count + sizeOfFile) <= MEMORY_SIZE) // если ЗП + наша задача меньше памяти, то добавляем
            {
                MEMORY.Enqueue(sizeOfFile);
                Console.WriteLine("Задача добавлена!");
            }
            else
            {
                QUEUE.Enqueue(sizeOfFile); //  иначе идет в очередь
                Console.WriteLine("В пямяти не хватило места! Перенесен в очередь!");
            }
            var outer = Task.Factory.StartNew(() =>      // внешняя задача
            {
                ProccesDo();
                var inner = Task.Factory.StartNew(() =>  // вложенная задача
                {
                    QueueDo();
                }, TaskCreationOptions.AttachedToParent);
            });
            outer.Wait(); // ожидаем выполнения внешней задачи
        }
        static void ProccesDelete()
        {
            int choice;
            Console.WriteLine($"Задач в памяти: {MEMORY.Count}");
            if (MEMORY.Count == 0)
            {
                Console.WriteLine("Вы ничего не можете удалить !");
                return;
            }
            else
            {
                Console.WriteLine("Какую задачу удалить? ");
                choice = Convert.ToInt32(Console.ReadLine());
                int queueCount = MEMORY.Count; // запоминаем длину очереди
                for (int i = 0; i < queueCount; i++)
                {
                    int currentFirstElement = MEMORY.Dequeue(); // каждый раз удаляем первый элемент в очереди и запоминаем его.

                    if (choice != currentFirstElement) // если значение этого первого элемента не то, которое нужно удалить, 
                    {
                        MEMORY.Enqueue(currentFirstElement); // то добавляем этот элемент обратно в очередь (в её конец).
                    }
                }
            }
        }
        static void Main(string[] args)
        {
            int choice = 0;
            Console.WriteLine("===Меню======================");
            Console.WriteLine("=  Состояние памяти -  1  =");
            Console.WriteLine("=  Состояние очереди - 2  =");
            Console.WriteLine("=  Добавить задачи -   3  =");
            Console.WriteLine("=  Убрать задачу -     4  =");
            Console.WriteLine("=  Выход -             0  =");
            Console.WriteLine("=============================");
            do
            {
                choice = Convert.ToInt32(Console.ReadLine());
                switch (choice)
                {
                    case 1:
                        {
                            MemoryInfo();
                        }
                        break;
                    case 2:
                        {
                            QueueInfo();
                        }
                        break;
                    case 3:
                        {
                            lock (locker)
                            {
                                Thread myThread = new Thread(ProccesAdd);
                                myThread.Start();
                            }
                        }
                        break;
                    case 4:
                        {
                            ProccesDelete();
                        }
                        break;
                    case 0:
                        {
                            return;
                        }
                        break;
                    default:
                        Console.WriteLine("! Введите правильные данные !");
                        break;
                }
            } while (choice != 0);
        }
    }
}
