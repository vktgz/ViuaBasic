.function: main/0
  vec %1 local
  fstore %2 local 3.4
  vpush %1 local %2 local
  vec %2 local
  istore %3 local 5
  vpush %2 local %3 local
  istore %3 local 6
  vpush %2 local %3 local
  vpush %1 local %2 local
  print %1 local
  vpop %2 local %1 local
  print %2 local
  vlen %3 local %2 local
  print %3 local
  vpop %2 local %1 local
  print %2 local
  vlen %3 local %2 local
  print %3 local
  print %1 local
  izero %0 local
  return
.end
