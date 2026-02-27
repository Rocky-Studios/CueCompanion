namespace CueCompanion;

public class Or<T1,T2>
{
    public T1? Option1 { get; set; }
    public T2? Option2 { get; set; }
    
    public Or()
    {
        Option1 = default(T1);
        Option2 = default(T2);
    }

    public Or(T1 option1)
    {
        Option1 = option1;
        Option2 = default(T2);
    }

    public Or(T2 option2)
    {
        Option1 = default(T1);
        Option2 = option2;
    }

    public Or(T1? option1, T2? option2)
    {
        Option1 = option1;
        Option2 = option2;
    }
}