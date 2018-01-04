.function: main/0
  fstore %2 local 4
  fstore %3 local -7
  if (eq %5 local %2 local (fstore %4 local 0)) +1 +2
  throw (strstore %5 local "modulo by zero")
  print %2 local
  print %3 local
  izero %0 local
  return
.end
