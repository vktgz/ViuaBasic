.function: main/0
  fstore %2 local -4
  fstore %3 local 7
  print (strstore %1 local "a")
  if (not (eq %5 local %2 local (fstore %4 local 0))) mod_not_zero
  throw (strstore %5 local "modulo by zero")
  .mark: mod_not_zero
  print (strstore %8 local "bbb")
  print %2 local
  .unused: %3
  izero %0 local
  return
.end
