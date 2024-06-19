public class CalculatorService: ICalculatorService
{
    public int Add(int a, int b)
    {
        return a + b;
    }
}

public interface ICalculatorService
{
    int Add(int a, int b);
}
