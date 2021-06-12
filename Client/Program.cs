using System;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Введите имя сервера");
            NamedPipeClientStream clientStream = new NamedPipeClientStream(Console.ReadLine()); 
            // Подключение к каналу с введенным названием 
            clientStream.Connect();
            Console.WriteLine("Подключение успешно!");
            byte[] fileBuffer = new byte[1024 * 1024];
            try
            {

                while (true)
                {
                    int length = clientStream.ReadByte(); // Считываем длину имени файла
                    
                    byte[] buffer = new byte[length * 2];
                    clientStream.Read(buffer, 0, buffer.Length); // Считывание имени файла
                    
                    string name = Encoding.Unicode.GetString(buffer);
                    clientStream.Read(buffer, 0, buffer.Length); // Считывание размера файла
                    long size = BitConverter.ToInt64(buffer, 0);


                    Console.WriteLine($"Принять файл {name} весом {((double)size / 1024 / 1024).ToString("F")}MB?(да/нет)");
                    
                    if (Console.ReadLine().ToLower()=="да")
                    {
                        Console.WriteLine("Начинается загрузка файла");
                        clientStream.WriteByte(1); // Подтверждаем загрузку файла
                        long readed = 0;
                        var fs = File.OpenWrite(name); // Открываем/создаем файл
                        while (readed < size) // Пока к-во считаных байтов мен
                        {
                            int count = clientStream.Read(fileBuffer, 0, fileBuffer.Length); // Считываем кусок файла  
                            fs.Write(fileBuffer, 0, count); // Пишем его в файл
                            readed += count;
                        }
                        fs.Close(); // Закрываем файл
                        Console.WriteLine("Файл загружен!");
                    }
                }
            }
            catch (Exception e)  // Если произошла ошибка
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
