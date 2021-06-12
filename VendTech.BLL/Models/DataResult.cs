namespace VendTech.BLL.Models
{
     public class DataResult<A, B, C>
     {
          public A Result1 { get; set; }
          public B Result2 { get; set; }
          public C Result3 { get; set; }
     }
    public class DataResult<A, B>
    {
        public A Result1 { get; set; }
        public B Result2 { get; set; }
    }
}
