.function: main/0
  float %2 local -4
  float %3 local 7
  print (string %1 local "a")
  if (not (eq %5 local %2 local (float %4 local 0))) mod_not_zero
  throw (string %5 local "modulo by zero")
  .mark: mod_not_zero
  print (string %8 local "bbb")
  print %2 local
  .unused: %3
  izero %0 local
  return
.end
