/*
При работе с массивами иногда их используют повторно, вместо того, чтобы удалить.
Это позволяет сэкономить на выделениях памяти.

Однако, в целях безопасности,
такие массивы очищяются (перезаписываются нулями) после выполнения группы вычислений,
так как для каждой группы операции результаты предыдущей не нужны

Проиллюстрируем, как может быть устроен такой класс, если использовать для него интерфейс IDisposable
*/

var a = new Array1();

// Имитируем какие-либо операции с массивом
// Операции могут производится через поле externalObject
using (var op = a.getArray())
{
    Console.WriteLine("getArray " + op.externalObject.LongLength);
}

using (var op = a.getArray())
{
    Console.WriteLine("getArray " + op.externalObject.LongLength);
}

using (var op = a.getArray())
{
    Console.WriteLine("getArray " + op.externalObject.LongLength);
}

/* Вывод программы:

getArray 1048576
a to null
getArray 1048576
a to null
getArray 1048576
a to null

*/



class Array1: OperationFinisherClient
{
    protected byte[] a = new byte[1024*1024];

    public OperationFinisher<byte[]> getArray()
    {
        return new OperationFinisher<byte[]>(this, a);
    }

    void OperationFinisherClient.Finished()
    {
        Console.WriteLine("a to null");
        Array.Fill<byte>(a, (byte) 0);
    }
}

interface OperationFinisherClient
{
    void Finished();
}

class OperationFinisher<T>: IDisposable
{
    private OperationFinisherClient obj;
    public readonly T externalObject;

    public OperationFinisher(OperationFinisherClient obj, T externalObject)
    {
        this.obj            = obj;
        this.externalObject = externalObject;
    }

    void IDisposable.Dispose()
    {
        obj.Finished();
    }
}
