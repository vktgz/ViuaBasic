.function: main/0
  string %1 local "#{0}"
  print %1 local
  integer %2 local 7
  print %2 local
  vector %3 local
  vpush %3 local %2 local
  print %3 local
  frame ^[(param %0 %1 local) (param %1 %3 local)]
  msg %4 format/
  print %4 local
  izero %0 local
  return
.end
