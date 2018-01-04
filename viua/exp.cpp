#include <iostream>
#include <iomanip>

double simple_exponent(double argum)
{
  double result = 1.0;
  double iks = argum;
  double silnia = 1.0;
  for (int i = 1; i <= 50; i++)
  {
    result = result + iks / silnia;
    std::cout << i << ' ' << std::setprecision(17) << result << std::endl;
    iks = iks * argum;
    silnia = silnia * (i + 1);
  }
  return result;
}

int main()
{
  double x = simple_exponent(7.6);
  std::cout << "exp(7.6) = 1998.19589510 : " << std::setprecision(17) << x << std::endl;
  return 0;
}
