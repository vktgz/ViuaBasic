.function: main/0
  fstore %2 local -4
  fstore %3 local 7
  ;%2 = %3 mod %2
  if (not (eq %5 local %2 local (fstore %4 local 0))) mod_not_zero
  throw (strstore %5 local "modulo by zero")
  .mark: mod_not_zero
  if (lt %5 local %2 local (fstore %4 local 0)) mod_negative
  fstore %6 local 0
  copy %7 local %2 local
  jump mod_prepare
  .mark: mod_negative
  copy %6 local %2 local
  fstore %7 local 0
  .mark: mod_prepare
  copy %8 local %2 local
  if (lt %5 local %3 local (fstore %4 local 0)) mod_check_step
  if (gt %5 local %8 local (fstore %4 local 0)) mod_negate_step mod_check
  .mark: mod_check_step
  if (gt %5 local %8 local (fstore %4 local 0)) mod_check
  .mark: mod_negate_step
  mul %8 local %8 local (fstore %4 local -1)
  .mark: mod_check
  if (not (gte %5 local %3 local %6 local)) mod_add
  if (lte %5 local %3 local %7 local) mod_done
  .mark: mod_add
  add %3 local %3 local %8 local
  jump mod_check
  .mark: mod_done
  copy %2 local %3 local
  print %2 local
  izero %0 local
  return
.end
