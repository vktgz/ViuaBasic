.function: main/0
  fstore %1 local 2.5 ; from
  fstore %2 local -8 ; to
  fstore %3 local -2 ; step
  copy %4 local %1 local ; idx = from
  .mark: for_1_begin
  if (lt %5 local %3 local (fstore %6 local 0)) for_1_descend
  if (gt %5 local %4 local %2 local) for_1_end
  jump for_1_step
  .mark: for_1_descend
  if (lt %5 local %4 local %2 local) for_1_end
  .mark: for_1_step
  text %5 local "IDX="
  text %6 local %4 local
  textconcat %5 local %5 local %6 local
  print %5 local
  add %4 local %4 local %3 local ; next
  jump for_1_begin
  .mark: for_1_end
  izero %0 local
  return
.end
