.function: main/0
  .unused: %7
  fstore %2 local 4
  fstore %3 local -7
  ;%2 = %3 mod %2
  if (eq %5 local %2 local (fstore %4 local 0)) +1 +3
  throw (strstore %5 local "modulo by zero")
  if (lt %5 local %2 local (fstore %4 local 0)) +1 +4
  copy %6 local %2 local
  fstore %7 local 0
  jump +3
  fstore %6 local 0
  copy %7 local %2 local
  copy %8 local %2 local
  if (lt %5 local %3 local (fstore %4 local 0)) +1 +2
  if (lt %5 local %8 local (fstore %4 local 0)) +2 +3
  if (gt %5 local %8 local (fstore %4 local 0)) +1 +2
  mul %8 local %8 local (fstore %4 local -1)
  if (gte %5 local %3 local %6 local) +1 +2
  if (lte %5 local %3 local %7 local) +3 +1
  add %3 local %3 local %8 local
  jump -3
  copy %2 local %3 local
  print %2 local
  izero %0 local
  return
.end
