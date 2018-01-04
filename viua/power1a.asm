.function: main/0
  ;%1 = %2 ^ %3
  fstore %2 local 7.6
  fstore %3 local 4.2
  frame ^[(param %0 %2 local) (param %1 %3 local)]
  call %1 local pow/2
  echo (strstore %4 local "7.6 ^ 4.2 = 5005.14988637 : ")
  print %1 local
  izero %0 local
  return
.end

.function: pow/2
  arg %1 local %0
  arg %2 local %1
  ;%0 = %1 ^ %2
  ;%1 ^ 1 = %1
  if (not (eq %3 local %2 local (fstore %4 local 1))) pow_pow_zero
  copy %0 local %1 local
  jump pow_done
  .mark: pow_pow_zero ;%2 = 0
  if (not (eq %3 local %2 local (fstore %4 local 0))) pow_pow_minus1
  ;0 ^ 0 is undefined
  if (not (eq %3 local %1 local (fstore %4 local 0))) pow_pow_zero_base_not_zero
  throw (strstore %5 local "0 ^ 0 is undefined")
  .mark: pow_pow_zero_base_not_zero
  ;%1 ^ 0 = 1
  fstore %0 local 1
  jump pow_done
  .mark: pow_pow_minus1 ;%2 = -1
  if (not (eq %3 local %2 local (fstore %4 local -1))) pow_base_plus1
  ;0 ^ -1 = 1 / 0
  if (not (eq %3 local %1 local (fstore %4 local 0))) pow_pow_minus1_base_not_zero
  throw (strstore %5 local "divide by zero")
  .mark: pow_pow_minus1_base_not_zero
  ;%1 ^ -1 = 1 / %1
  div %0 local (fstore %4 local 1) %1 local
  jump pow_done
  .mark: pow_base_plus1 ;%1 = 1
  if (not (eq %3 local %1 local (fstore %4 local 1))) pow_base_minus1
  ;1 ^ %2 = 1
  fstore %0 local 1
  jump pow_done
  .mark: pow_base_minus1 ;%1 = -1
  if (not (eq %3 local %1 local (fstore %4 local -1))) pow_pow_int
  ;-1 ^ %2 = 1 or -1
  frame ^[(param %0 %2 local)]
  call %6 local abs/1
  frame ^[(param %0 %6 local) (param %1 (fstore %4 local 2))]
  call %6 local mod/2
  if (eq %3 local %6 local (fstore %4 local 0)) pow_base_minus1_positive
  fstore %0 local -1
  jump pow_done
  .mark: pow_base_minus1_positive
  fstore %0 local 1
  jump pow_done
  .mark: pow_pow_int ;%2 is integer
  if (not (eq %3 local %2 local (ftoi %4 local %2 local))) pow_other
  if (lte %3 local %2 local (fstore %4 local 0)) pow_pow_int_negative
  ;%1 ^ int = %1 * %1 * ... * %1
  frame ^[(param %0 %1 local) (param %1 %2 local)]
  call %0 local simple_pow/2
  jump pow_done
  .mark: pow_pow_int_negative
  ;0 ^ -int = 1 / (0 * 0 * ... * 0)
  if (not (eq %3 local %1 local (fstore %4 local 0))) pow_pow_int_negative_base_not_zero
  throw (strstore %5 local "divide by zero")
  .mark: pow_pow_int_negative_base_not_zero
  mul %6 local %2 local (fstore %4 local -1)
  frame ^[(param %0 %1 local) (param %1 %6 local)]
  call %6 local simple_pow/2
  div %0 local (fstore %4 local 1) %6 local
  jump pow_done
  .mark: pow_other
  if (lt %3 local %1 local (fstore %4 local 0)) pow_other_base_negative
  frame ^[(param %0 %1 local) (param %1 %2 local)]
  call %0 local complicated_pow/2
  jump pow_done
  .mark: pow_other_base_negative
  ;negative ^ float = complex
  throw (strstore %5 local "result is complex number")
  .mark: pow_done
  return
.end

.function: simple_pow/2
  arg %1 local %0
  arg %2 local %1
  ;%0 = %1 ^ %2 = %1 * %1 * ... * %1 (%2 times)
  fstore %0 local 1
  istore %4 local 1
  .mark: spow_loop
  mul %0 local %0 local %1 local
  iinc %4 local
  if (lte %3 local %4 local %2 local) spow_loop
  return
.end

.function: complicated_pow/2
  arg %1 local %0
  arg %2 local %1
  ;%0 = %1 ^ %2
  frame ^[(param %0 %2 local)]
  call %3 local abs/1
  ftoi %4 local %3 local
  sub %5 local %3 local %4 local
  frame ^[(param %0 %1 local) (param %1 %4 local)]
  call %0 local simple_pow/2
  frame ^[(param %0 %1 local)]
  call %6 local log/1
  mul %6 local %5 local %6 local
  frame ^[(param %0 %6 local)]
  call %6 local exp/1
  mul %0 local %0 local %6 local
  if (gt %7 local %2 local (fstore %8 local 0)) cpow_done
  div %0 local (fstore %8 local 1) %0 local
  .mark: cpow_done
  return
.end

.function: abs/1
  arg %0 local %0
  if (gte %1 local %0 local (fstore %2 local 0)) abs_done
  mul %0 local %0 local (fstore %2 local -1)
  .mark: abs_done
  return
.end

.function: exp/1
  arg %1 local %0
  fstore %0 local 1
  copy %2 local %1 local
  fstore %3 local 1
  istore %4 local 1
  istore %7 local 900
  .mark: exp_loop
  div %5 local %2 local %3 local
  add %0 local %0 local %5 local
  mul %2 local %2 local %1 local
  iinc %4 local
  mul %3 local %3 local %4 local
  if (lte %6 local %4 local %7 local) exp_loop
  return
.end

.function: log/1
  arg %1 local %0
  if (gt %3 local %1 local (fstore %2 local 0)) log_positive
  throw (strstore %4 local "logarithm argument must be greater than zero")
  .mark: log_positive
  ;keep log(1.9) in global register
  if (not (isnull %3 local %1 global)) log_begin
  frame ^[(param %0 (fstore %2 local 1.9))]
  call %1 global series_log/1
  .mark: log_begin
  fstore %0 local 0
  ;if (x >= 2) then log(x) = (n * log(1.9)) + log(rest)
  if (lt %3 local %1 local (fstore %2 local 2)) log_rest
  istore %4 local 0
  fstore %5 local 1.9
  fstore %6 local 2
  .mark: log_divide
  div %1 local %1 local %5 local
  iinc %4 local
  if (gte %3 local %1 local %6 local) log_divide
  mul %0 local %1 global %4 local
  .mark: log_rest
  frame ^[(param %0 %1 local)]
  call %2 local series_log/1
  add %0 local %0 local %2 local
  return
.end

.function: series_log/1
  ;if (x > 0) and (x < 2) then log(x) = (x-1) - (x-1)^2/2 + (x-1)^3/3 - (x-1)^4/4 + ...
  arg %1 local %0
  sub %1 local %1 local (fstore %2 local 1)
  copy %2 local %1 local
  fstore %3 local 1
  fstore %0 local 0
  istore %4 local 1
  istore %5 local 10000
  .mark: series_log_loop
  div %6 local %2 local %4 local
  mul %6 local %3 local %6 local
  add %0 local %0 local %6 local
  mul %2 local %2 local %1 local
  mul %3 local %3 local (fstore %6 local -1)
  iinc %4 local
  if (lte %7 local %4 local %5 local) series_log_loop
  return
.end

.function: mod/2 ; result = arg0 mod arg1
  arg %2 local %1
  arg %3 local %0
  ;%0 = %3 mod %2
  if (not (eq %5 local %2 local (fstore %4 local 0))) mod_not_zero
  throw (strstore %1 local "modulo by zero")
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
  copy %0 local %3 local
  return
.end
