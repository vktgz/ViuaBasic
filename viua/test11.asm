.function: main/0
  istore %1 local 2
  fstore %1 global 3.4
  echo (strstore %9 local "a = ")
  print %1 local
  echo (strstore %9 local "b = ")
  print %1 global
  mul %2 local %1 local %1 global
  echo (strstore %9 local "a * b = ")
  print %2 local
  izero %0 local
  return
.end
