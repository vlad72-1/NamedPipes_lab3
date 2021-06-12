using System;
using System.IO;
using System.IO.Pipes;
using System.Text;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Введите имя сервера:");
            NamedPipeServerStream server = new NamedPipeServerStream(Console.ReadLine(), PipeDirection.InOut); // Создание канала
            server.WaitForConnection(); // Ожидание подключения
            Console.WriteLine("Клиент подключен"); 
             
            try
            {
                 
                while (true)
                {
                    Console.WriteLine("Введите путь к файлу:");
                    var file = Console.ReadLine();
                    if (!File.Exists(file)) // Проверка наличия файла
                        continue;
                    
                    string fileName = Path.GetFileName(file); // Получаем название файла без его точного расположения

                    server.WriteByte((byte)fileName.Length); // Записываем длину названия файла в поток
                    var fileNameBytes = Encoding.Unicode.GetBytes(fileName); // Конвертируем строку в массив байт
                    server.Write(fileNameBytes, 0, fileNameBytes.Length); // Пишем строку в поток
                    
                    using (var stream = File.OpenRead(file)) // Открываем файл
                    {
                        var lengthBytes = BitConverter.GetBytes(stream.Length); // Конвертируем длину файла в массив байт
                        server.Write(lengthBytes, 0, lengthBytes.Length); // Записываем его в поток
                        if (server.ReadByte() == 1) // Если клиент принял
                        {
                            Console.WriteLine("Отправка файла");
                            while (stream.Position < stream.Length) // Пока есть что читать
                            {
                                stream.CopyTo(server, 1024 * 1024); // Копируем из файлового потока в поток клиента
                            }
                            Console.WriteLine("Файл отправлен");
                        }
                    } }
            }
            catch (Exception e) // Если произошла ошибка
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
