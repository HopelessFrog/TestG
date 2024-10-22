namespace TestG.Consts;

public static class ErrorMessages
{
    public static string ParamNumberError = "Число параметров не соответствует ожидаемому и должно быть, как минимум 5!";
    public static string LeftLimitError = "Левая граница диапазона должна быть < правой границы диапазона!";
    public static string StepError = "Шаг интегрирования должен быть в пределах [0.000001;0.5]";
    public static string MethodError = "Четвертый параметр определяет метод интегрирования и должен быть в пределах [1;3]";
}
