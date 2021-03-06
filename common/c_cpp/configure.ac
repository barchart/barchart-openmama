# $Id$
#
# OpenMAMA: The open middleware agnostic messaging API
# Copyright (C) 2011 NYSE Technologies, Inc.
#
# This library is free software; you can redistribute it and/or
# modify it under the terms of the GNU Lesser General Public
# License as published by the Free Software Foundation; either
# version 2.1 of the License, or (at your option) any later version.
#
# This library is distributed in the hope that it will be useful,
# but WITHOUT ANY WARRANTY; without even the implied warranty of
# MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
# Lesser General Public License for more details.
#
# You should have received a copy of the GNU Lesser General Public
# License along with this library; if not, write to the Free Software
# Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA
# 02110-1301 USA
#


##################################################
#
# VERSION INFO
#
#################################################
m4_define([product_version_major],     [2])
m4_define([product_version_minor],     [2])
m4_define([product_version_release],   [1.1])
m4_define([product_full_version],[product_version_major.product_version_minor.product_version_release])


##################################################
#
# INTIALISATION
#
##################################################
AC_INIT(wombatcommon, [product_full_version], api@wombatconsulting.com)
AC_CONFIG_SRCDIR([src/c/machine.c])
AC_CONFIG_AUX_DIR([etc])
AM_INIT_AUTOMAKE([foreign])
AM_CONFIG_HEADER([config.h])
AC_CANONICAL_BUILD()

# Set up symbolic links for os-specific files
AC_MSG_NOTICE([Configuring symbolic links for $build_os in $srcdir])
case $build_os in
       linux*-*)   AC_CONFIG_LINKS([$srcdir/src/c/wombat/port.h:src/c/linux/port.h
				    $srcdir/src/c/wombat/wMath.h:src/c/linux/wMath.h
				    $srcdir/src/c/wombat/wUuid.h:src/c/linux/wUuid.h
				    $srcdir/src/c/wombat/wConfig.h:src/c/linux/wConfig.h
				    $srcdir/src/c/wombat/wInterlocked.h:src/c/linux/wInterlocked.h
				    $srcdir/src/c/machine_port.c:src/c/linux/machine.c
				    $srcdir/src/c/platform.c:src/c/linux/platform.c
				    $srcdir/src/c/wSemaphore.c:src/c/linux/wSemaphore.c
				    $srcdir/src/c/network.c:src/c/linux/network.c
				    $srcdir/src/c/environment.c:src/c/linux/environment.c]) ;;
esac

# Checks for programs.
AC_PROG_CXX

AC_PROG_LEX
if test "x$LEX" != xflex; then
    AC_ERROR([flex not found: common libraries require flex.])
fi

AC_PROG_CC

# Check whether to use built-in library archiver if C++ compiler is Sun/Forte
if CXXbase=`echo ${CXX} | ${ac_cv_prog_AWK} '{ print $1; }'` ; \
   test "x${build_vendor}" = "xsun" -a "x`basename ${CXXbase}`" = "xCC"
then
  AR="${CXX} -xar"
  AR_FLAGS="-o"
fi

# Use libtool:
AM_DISABLE_SHARED
AC_PROG_LIBTOOL
AM_PROG_LIBTOOL
AC_SUBST(LTLIBOBJS)

AC_LANG([C])
AC_LANG([C++])

# Default paths.
AC_PREFIX_DEFAULT([/opt/openmama])

# Flush all cached values to the cache file
AC_CACHE_SAVE

##################################################

##################################################
# UNIT TESTS: --with-unittests
# Whether or not to build the unit tests
# GTEST_HOME is the root directory of the gtest installation. It can be
# specified as an environment variable or as a configure parameter
##################################################
UNIT_TEST_HOME=[$GTEST_HOME]
AC_ARG_WITH(
    unittests,
    [AC_HELP_STRING([--with-unittests=DIR],
                    [Location of the gtest API include/library files in DIR])],
    [
     if test -d $withval; then
         UNIT_TEST_HOME="$withval"
     fi
     CPPFLAGS="$CPPFLAGS -I$UNIT_TEST_HOME/include -I$UNIT_TEST_HOME -DWITH_UNITTESTS "
     AC_CHECK_HEADERS(
         [$UNIT_TEST_HOME/include/gtest/gtest.h],
         [
          LDFLAGS="$LDFLAGS -L$UNIT_TEST_HOME/lib"

      with_unittests=true
         ],
         [
            AC_MSG_ERROR([Valid GTEST installation not found!])
         ]
         )
    ],
    [
     with_unittests=false
    ])
AM_CONDITIONAL(WITH_UNITTESTS, test x$with_unittests = xtrue)

##################################################

##################################################
#
# PREREQUISITES
#
##################################################
# Check for standard header files.
AC_HEADER_STDC
AC_CHECK_HEADERS([stdlib.h string.h unistd.h])

# Check whether we can use gcc specific compiler flags
AM_CONDITIONAL(USE_GCC_FLAGS, test "x${GCC}" = "xyes")

# Checks for typedefs, structures, and compiler characteristics.
AC_C_CONST

# Checks for library functions.
AC_FUNC_MALLOC
AC_CHECK_FUNCS([strdup])
AC_CHECK_FUNCS([socket])


##################################################
#
# OPTIONS
#
###################################################
##################################################

##################################################

##################################################
#
# VERSION FILE GENERATION
#
##################################################
mkdir -p src/c
echo "#ifndef WombatCommonVersionH" >  src/c/version.h
echo "#define WombatCommonVersionH" >> src/c/version.h
echo "extern const char wombatcommon_version[];" >> src/c/version.h
echo "#define WOMBAT_COMMON_VERSION() { const char* version = \"@(#) ${PACKAGE_NAME} ${PACKAGE_VERSION}\"; }" >> src/c/version.h
echo "#endif" >> src/c/version.h

##################################################
#
# FINAL OUTPUT
#
##################################################
AC_OUTPUT([Makefile \
			src/Makefile \
			src/c/Makefile \
			src/cpp/Makefile \
			])
