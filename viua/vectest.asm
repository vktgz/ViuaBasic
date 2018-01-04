.function: main/0
  vec %1 local
  vpush %1 local (istore %2 local 1)
  vpush %1 local (istore %2 local 2)
  vpush %1 local (istore %2 local 3)
  istore %9 local 1
  print %1 local
  vpop %0 local %1 local %9 local
  print %1 local
  vinsert %1 local (istore %2 local 4) %9 local
  print %1 local
  izero %0 local
  return
.end
