#ifndef GOLDCPP_STRING_H
#define GOLDCPP_STRING_H

#include <string>

#ifndef __GNUC__
  #define GPSTR_T       std::string
  #define GPSTR_C(str)  str
  #define GPCHR_T       char
#else
  #define GPSTR_T       std::u16string
  #define GPSTR_C(str)  u##str
  #define GPCHR_T       char16_t
#endif

#endif // GOLDCPP_STRING_H

