.function: main/0
  vector %1 local
  float %2 local 3.4
  vpush %1 local %2 local
  vector %2 local
  integer %3 local 5
  vpush %2 local %3 local
  integer %3 local 6
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
