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

srcdir = @srcdir@
VPATH  = @srcdir@
blddir=@builddir@


if USE_GCC_FLAGS
CFLAGS   += -std=gnu99 -pedantic -Wno-long-long -O2 -pthread -fPIC
CPPFLAGS += -pedantic -Wno-long-long -O2 -pthread -fPIC
endif

INCLUDES = -I$(srcdir)/.. -I$(srcdir)/../.. -I$(srcdir)/../../../examples -I$(srcdir)/../../../../../common/c_cpp/src/c

CFLAGS   += -I@builddir@/../../c
CPPFLAGS += -I@builddir@/../../c


LDFLAGS  += -L${srcdir}/../../c \
            -L${srcdir}/../../../../../common/c_cpp/src/c

LIBS = -lmama -lwombatcommon -lgtest
LDADD = -lgtest_main -ldl

bin_PROGRAMS = UnitTestMamaC

nodist_UnitTestMamaC_SOURCES = MainUnitTestC.cpp \
							msgtests.cpp \
							iteration.cpp \
                            openclosetest.cpp \
                            datetimetest.cpp \
                            subscriptiontest.cpp \
                            pricetest.cpp \
                            timertest.cpp

