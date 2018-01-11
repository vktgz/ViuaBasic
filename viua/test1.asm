.function: main/0
  text %1 local "Hello World!"
  print %1 local
  string %1 local "#{0}"
  integer %2 local 2
  print %2 local
  text %3 local "A1="
  integer %4 local 7
  text %4 local %4 local
  textconcat %4 local %3 local %4 local
  print %4 local
  text %4 local ""
  print %4 local
  izero %0 local
  return
.end
