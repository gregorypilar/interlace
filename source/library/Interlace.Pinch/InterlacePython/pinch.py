# Copyright (c) 2007-2012, Computer Consultancy Pty Ltd
# All rights reserved.
# 
# Redistribution and use in source and binary forms, with or without
# modification, are permitted provided that the following conditions are met:
#     * Redistributions of source code must retain the above copyright
#       notice, this list of conditions and the following disclaimer.
#     * Redistributions in binary form must reproduce the above copyright
#       notice, this list of conditions and the following disclaimer in the
#       documentation and/or other materials provided with the distribution.
#     * Neither the name of the Computer Consultancy Pty Ltd nor the
#       names of its contributors may be used to endorse or promote products
#       derived from this software without specific prior written permission.
# 
# THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" 
# AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
# IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
# ARE DISCLAIMED. IN NO EVENT SHALL COMPUTER CONSULTANCY PTY LTD BE LIABLE 
# FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL 
# DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR 
# SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER 
# CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT 
# LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY 
# OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
# DAMAGE.

import struct, os, cStringIO

PACKED_BYTE_KIND_MASK = 0xc0
PACKED_BYTE_VALUE_MASK = 0x3f

PACKED_SEQUENCE_BYTE = 0xc0
PACKED_PRIMATIVE_BUFFER_BYTE = 0x80
PACKED_PRIMATIVE_ORDINAL_BYTE = 0x40

TAGGED_SEQUENCE_BYTE = 0x03
TAGGED_PRIMATIVE_BUFFER_BYTE = 0x02
TAGGED_PRIMATIVE_ORDINAL_BYTE = 0x01

TAGGED_CHOICE_BYTE = 0x04

NULL = 0x00

TOKEN_SEQUENCE = 1 # The argument is the number of tokens in the sequence, which must then be read.
TOKEN_PRIMITIVE_BUFFER = 2 # The argument is the number of bytes that follow in the buffer.
TOKEN_PRIMITIVE_PACKED_ORDINAL = 3 # The argument is the ordinal value; nothing needs to be read.
TOKEN_PRIMITIVE_TAGGED_ORDINAL = 4 # No argument; the ordinal value must be read.
TOKEN_CHOICE = 5 # The value kind is the argument; the structure follows.
TOKEN_NULL = 6 # No argument and nothing to read after.

class PinchException(Exception):
	pass

class PinchEndOfStreamException(PinchException):
	pass

class PinchInvalidCodingException(PinchException):
	pass

class PinchNullRequiredFieldException(PinchException):
	pass

class PinchInvalidFieldTypeException(PinchException):
	pass

class PinchDecodingContext(object):
	pass

class PinchDecimal(object):
	def __init__(self, sign, digits, exponent):
		self.sign = sign
		self.digits = digits
		self.exponent = exponent

	def __eq__(self, rhs):
		if not isinstance(rhs, PinchDecimal): return False

		return \
			self.sign == rhs.sign and \
			self.digits == rhs.digits and \
			self.exponent == rhs.exponent

class PinchDecoder(object):
	def __init__(self, file):
		self._file = file
		self._file_can_seek = True

		self._read_required = True
		self._read_token_kind = None
		self._read_token_argument = None

	# Decoding Primitives:

	def _read_token_internal(self):
		read_byte = self._file.read(1)

		if read_byte == "": raise PinchEndOfStreamException()

		token_byte = ord(read_byte)
		masked_token_byte = token_byte & PACKED_BYTE_KIND_MASK

		if masked_token_byte == PACKED_PRIMATIVE_ORDINAL_BYTE:
			self._read_token_kind = TOKEN_PRIMITIVE_PACKED_ORDINAL
			self._read_token_argument = token_byte & PACKED_BYTE_VALUE_MASK

		elif masked_token_byte == PACKED_PRIMATIVE_BUFFER_BYTE:
			self._read_token_kind = TOKEN_PRIMITIVE_BUFFER
			self._read_token_argument = token_byte & PACKED_BYTE_VALUE_MASK

		elif masked_token_byte == PACKED_SEQUENCE_BYTE:
			self._read_token_kind = TOKEN_SEQUENCE
			self._read_token_argument = token_byte & PACKED_BYTE_VALUE_MASK

		elif token_byte == NULL:
			self._read_token_kind = TOKEN_NULL
			self._read_token_argument = None

		elif token_byte == TAGGED_PRIMATIVE_ORDINAL_BYTE:
			self._read_token_kind = TOKEN_PRIMITIVE_TAGGED_ORDINAL
			self._read_token_argument = None

		elif token_byte == TAGGED_PRIMATIVE_BUFFER_BYTE:
			self._read_token_kind = TOKEN_PRIMITIVE_BUFFER
			self._read_token_argument = self.read_unsigned_tag()

		elif token_byte == TAGGED_SEQUENCE_BYTE:
			self._read_token_kind = TOKEN_SEQUENCE
			self._read_token_argument = self.read_unsigned_tag()

		elif token_byte == TAGGED_CHOICE_BYTE:
			self._read_token_kind = TOKEN_CHOICE
			self._read_token_argument = self.read_unsigned_tag()

	def _peek_token(self):
		if self._read_required: self._read_token_internal()

		self._read_required = False

		return self._read_token_kind

	def _read_token(self):
		if self._read_required: self._read_token_internal()

		self._read_required = True

		return self._read_token_kind

	# Decoding Utilities:

	def _read_bytes(self, count):
		bytes = self._file.read(count)

		if len(bytes) < count: raise PinchEndOfStreamException()

		return bytes

	def _skip_bytes(self, count):
		if count > 32 and self._file_can_seek:
			try:
				self._file.seek(count, os.SEEK_CUR)

				return
			except IOError:
				self._file_can_seek = False

		total_read = 0

		while total_read < count:
			read = self._read_bytes(min(count, 1024))

			total_read += len(read)

	def _read_unsigned_tag(self):
		shift = 0
		tag = 0

		while True:
			read_byte = self._file.read(1)

			if read_byte == "": raise PinchEndOfStreamException()

			read_byte = ord(read_byte)

			if shift == 28:
				if read_byte & 0x70: raise PinchInvalidCodingException()
				if read_byte & 0x08: tag = long(tag)
			elif shift > 28: raise PinchInvalidCodingException()

			tag |= (read_byte & 0x7f) << shift

			shift += 7

			if (read_byte & 0x80) == 0: break

		return tag

	def _read_signed_tag(self):
		shift = 0
		tag = 0

		while True:
			read_byte = self._file.read(1)

			if read_byte == "": raise PinchEndOfStreamException()

			read_byte = ord(read_byte)

			if shift == 28:
				if read_byte & 0x70: raise PinchInvalidCodingException()
				if read_byte & 0x08: tag = long(tag)
			elif shift > 28: raise PinchInvalidCodingException()

			tag |= (read_byte & 0x7f) << shift

			shift += 7

			if (read_byte & 0x80) == 0: break

		if tag & 1:
			return int(-(tag >> 1) - 1)
		else:
			return int(tag >> 1)

	def _read_signed_long_tag(self): 
		shift = 0
		tag = 0L

		while True:
			read_byte = self._file.read(1)

			if read_byte == "": raise PinchEndOfStreamException()

			read_byte = ord(read_byte)

			if shift == 63:
				if read_byte & 0x7e: raise PinchInvalidCodingException()
			elif shift > 63: raise PinchInvalidCodingException()

			tag |= (read_byte & 0x7f) << shift

			shift += 7

			if (read_byte & 0x80) == 0: break

		if tag & 1:
			return -(tag >> 1) - 1
		else:
			return tag >> 1

	def _skip_tag(self):
		while True:
			read_byte = self._file.read(1)

			if read_byte == "": raise PinchEndOfStreamException()

			if (ord(read_byte) & 0x80) == 0: break

	def _read_decimal(self): # decimal 
		# Read the buffer token and length, ensuring it has enough bytes to be valid:
		token = self._read_token()

		if token != TOKEN_PRIMITIVE_BUFFER or self._read_token_argument < 2:
			raise PinchInvalidCodingException()

		bytes = self._read_bytes(self._read_token_argument)

		exponent = ord(bytes[0])
		is_negative = (ord(bytes[1]) & 0x80) != 0

		buffer_length = self._read_token_argument
		buffer_used = 2

		shift = 0
		value = 0L

		while buffer_used != buffer_length:
			read_byte = ord(bytes[buffer_used])
			buffer_used += 1

			value |= read_byte << shift

			shift += 8

		return PinchDecimal(is_negative, value, exponent)

	# Decoding Assistants:

	def _read_primitive_buffer(self):
		token = self._read_token()

		if token != TOKEN_PRIMITIVE_BUFFER: raise PinchInvalidCodingException()

		return self._read_bytes(self._read_token_argument)

	def _read_primitive_buffer_with_expect(self, expected):
		token = self._read_token()

		if token != TOKEN_PRIMITIVE_BUFFER: raise PinchInvalidCodingException()

		if expected != self._read_token_argument:
			raise PinchInvalidCodingException()

		return self._read_bytes(self._read_token_argument)

	def _read_primitive_signed_ordinal(self):
		token = self._read_token()

		if token == TOKEN_PRIMITIVE_PACKED_ORDINAL:
			return self._read_token_argument
		elif token == TOKEN_PRIMITIVE_TAGGED_ORDINAL:
			return self._read_signed_tag()
		else:
			raise PinchInvalidCodingException()

	def _read_primitive_unsigned_ordinal(self):
		token = self._read_token()

		if token == TOKEN_PRIMITIVE_PACKED_ORDINAL:
			return self._read_token_argument
		elif token == TOKEN_PRIMITIVE_TAGGED_ORDINAL:
			return self._read_unsigned_tag()
		else:
			raise PinchInvalidCodingException()

	def _read_primitive_long_ordinal(self):
		token = self._read_token()

		if token == TOKEN_PRIMITIVE_PACKED_ORDINAL:
			return long(self._read_token_argument)
		elif token == TOKEN_PRIMITIVE_TAGGED_ORDINAL:
			return self._read_signed_long_tag()
		else:
			raise PinchInvalidCodingException()

	# Decoder Implementation:

	def open_sequence(self):
		token = self._read_token()

		if token != TOKEN_SEQUENCE: raise PinchInvalidCodingException()

		return self._read_token_argument

	def decode_choice_marker(self):
		token = self._read_token()

		if token != TOKEN_CHOICE: raise PinchInvalidCodingException()

		return self._read_token_argument

	def decode_required_float32(self, properties):
		return struct.unpack("!f", self._read_primitive_buffer_with_expect(4))[0]

	def decode_required_float64(self, properties):
		return struct.unpack("!d", self._read_primitive_buffer_with_expect(8))[0]

	def decode_required_int8(self, properties):
		return self._read_primitive_unsigned_ordinal()

	def decode_required_int16(self, properties):
		return self._read_primitive_signed_ordinal()

	def decode_required_int32(self, properties):
		return self._read_primitive_signed_ordinal()

	def decode_required_int64(self, properties):
		return self._read_primitive_long_ordinal()

	def decode_required_decimal(self, properties):
		return self._read_decimal()

	def decode_required_bool(self, properties):
		return self._read_primitive_unsigned_ordinal() == 1

	def decode_required_string(self, properties):
		bytes = self._read_primitive_buffer()

		return unicode(bytes, "utf-8")

	def decode_required_bytes(self, properties):
		return self._read_primitive_buffer()

	def decode_required_enumeration(self, properties):
		return self._read_primitive_unsigned_ordinal()

	def decode_required_structure(self, factory, properties):
		value = factory(PinchDecodingContext())

		value.decode(self)

		return value

	def _start_optional(self):
		if self._peek_token() != TOKEN_NULL:
			return True
		else:
			if self._read_token() != TOKEN_NULL: raise ValueError()

			return False

	def decode_optional_float32(self, properties):
		if self._start_optional():
			return struct.unpack("!f", self._read_primitive_buffer_with_expect(4))[0]
		else:
			return None

	def decode_optional_float64(self, properties):
		if self._start_optional():
			return struct.unpack("!d", self._read_primitive_buffer_with_expect(8))[0]
		else:
			return None

	def decode_optional_int8(self, properties):
		if self._start_optional():
			return self._read_primitive_unsigned_ordinal()
		else:
			return None

	def decode_optional_int16(self, properties):
		if self._start_optional():
			return self._read_primitive_signed_ordinal()
		else:
			return None

	def decode_optional_int32(self, properties):
		if self._start_optional():
			return self._read_primitive_signed_ordinal()
		else:
			return None

	def decode_optional_int64(self, properties):
		if self._start_optional():
			return self._read_primitive_long_ordinal()
		else:
			return None

	def decode_optional_decimal(self, properties):
		if self._start_optional():
			return self._read_decimal()
		else:
			return None

	def decode_optional_bool(self, properties):
		if self._start_optional():
			return self._read_primitive_unsigned_ordinal() == 1
		else:
			return None

	def decode_optional_string(self, properties):
		if self._start_optional():
			bytes = self._read_primitive_buffer()

			return unicode(bytes, "utf-8")
		else:
			return None

	def decode_optional_bytes(self, properties):
		if self._start_optional():
			return self._read_primitive_buffer()
		else:
			return None

	def decode_optional_enumeration(self, properties):
		if self._start_optional():
			return self._read_primitive_unsigned_ordinal()
		else:
			return None

	def decode_optional_structure(self, factory, properties):
		if self._start_optional():
			value = factory(PinchDecodingContext())

			value.decode(self)

			return value
		else:
			return None

	def skip_fields(self, remaining_fields):
		for i in range(remaining_fields):
			token = self._read_token

			if token == TOKEN_SEQUENCE:
				self._skip_fields(self._read_token_argument)

			elif token == TOKEN_PRIMITIVE_BUFFER:
				self._skip_bytes(self._read_token_argument)

			elif token == TOKEN_PRIMITIVE_PACKED_ORDINAL:
				pass

			elif token == TOKEN_PRIMITIVE_TAGGED_ORDINAL:
				self._skip_tag()

			elif token == TOKEN_CHOICE:
				self._skip_tag()
				self._skip_fields(1)

			elif token == TOKEN_NULL:
				pass

	def skip_removed(self):
		self.skip_fields(1)

	def close_sequence(self):
		pass

class PinchEncoder(object):
	def __init__(self, file):
		self._file = file

	# Encoding Utilities:

	def _write_unsigned_tag(self, tag):
		if tag < 0 or 4294967296L <= tag:
			raise PinchInvalidCodingException()

		remaining = tag

		while remaining > 0x7f:
			self._file.write(chr((remaining & 0xff) | 0x80))
			remaining = remaining >> 7

		self._file.write(chr(remaining))

	def _write_signed_tag(self, tag):
		if tag < -2147483648 or 2147483647 < tag:
			raise PinchInvalidCodingException()

		if tag < 0:
			tag = -tag - 1

			if not tag & 0x40000000:
				remaining = (int(tag) << 1) | 1
			else:
				remaining = (long(tag) << 1) | 1
		else:
			if not tag & 0x40000000:
				remaining = (int(tag) << 1) 
			else:
				remaining = (long(tag) << 1) 

		while remaining > 0x7f:
			self._file.write(chr((remaining & 0xff) | 0x80))
			remaining = remaining >> 7

		self._file.write(chr(remaining))

	def _write_signed_long_tag(self, tag):
		if tag < 0:
			remaining = ((-long(tag) - 1) << 1) | 1
		else:
			remaining = tag << 1

		while remaining > 0x7f:
			self._file.write(chr((remaining & 0xff) | 0x80))
			remaining = remaining >> 7

		self._file.write(chr(remaining))

	# Encoding Primatives:

	def _write_sequence_marker(self, count):
		if count < 64:
			self._file.write(chr(PACKED_SEQUENCE_BYTE | count))
		else:
			self._file.write(chr(TAGGED_SEQUENCE_BYTE))

			self._write_unsigned_tag(count)

	def _write_primative_buffer(self, buffer):
		if len(buffer) < 64:
			self._file.write(chr(PACKED_PRIMATIVE_BUFFER_BYTE | len(buffer)))
		else:
			self._file.write(chr(TAGGED_PRIMATIVE_BUFFER_BYTE))
		
			self._write_unsigned_tag(len(buffer))

		self._file.write(buffer)

	def _write_primative_signed_ordinal(self, ordinal):
		if 0 <= ordinal < 64:
			self._file.write(chr(PACKED_PRIMATIVE_ORDINAL_BYTE | ordinal))
		else:
			self._file.write(chr(TAGGED_PRIMATIVE_ORDINAL_BYTE))

			self._write_signed_tag(ordinal)

	def _write_primative_unsigned_ordinal(self, ordinal):
		if 0 <= ordinal < 64:
			self._file.write(chr(PACKED_PRIMATIVE_ORDINAL_BYTE | ordinal))
		else:
			self._file.write(chr(TAGGED_PRIMATIVE_ORDINAL_BYTE))

			self._write_unsigned_tag(ordinal)

	def _write_primative_long_ordinal(self, ordinal):
		if 0 <= ordinal < 64:
			self._file.write(chr(PACKED_PRIMATIVE_ORDINAL_BYTE | ordinal))
		else:
			self._file.write(chr(TAGGED_PRIMATIVE_ORDINAL_BYTE))

			self._write_signed_long_tag(ordinal)

	def _write_null(self):
		self._file.write(chr(NULL))

	def encode_choice_marker(self, value_kind):
		self._file.write(chr(TAGGED_CHOICE_BYTE))

		self._write_unsigned_tag(value_kind)

	def encode_removed(self):
		self._file.write(chr(NULL))

	def _write_decimal(self, value):
		# Validate the decimal and determine the number of bytes to encode it:
		if not isinstance(value, PinchDecimal): raise PinchInvalidCodingException()
		if not 0 <= value.exponent <= 255: raise PinchInvalidCodingException()
		if not value.digits >= 0: raise PinchInvalidCodingException()

		digit_octet_count = 1
		digits = value.digits >> 8

		while digits > 0:
			digit_octet_count += 1
			digits = digits >> 8

			if digit_octet_count > 8: raise PinchInvalidCodingException()

		# Write the tag and encoded decimal:
		self._file.write(chr(PACKED_PRIMATIVE_BUFFER_BYTE | (digit_octet_count + 2)))
		self._file.write(chr(value.exponent))

		if value.sign:
			self._file.write(chr(0x80))
		else:
			self._file.write(chr(0x00))

		digits = value.digits

		self._file.write(chr(digits & 0xff))
		digits = digits >> 8

		while digits > 0:
			self._file.write(chr(digits & 0xff))
			digits = digits >> 8

	# Decoder Members:

	def open_sequence(self, count):
		if count < 0: raise ValueError()

		self._write_sequence_marker(count)

	def encode_required_float32(self, value, properties):
		if type(value) not in (int, long, float):
			raise PinchInvalidCodingException()

		self._write_primative_buffer(struct.pack("!f", float(value)))

	def encode_required_float64(self, value, properties):
		if type(value) not in (int, long, float):
			raise PinchInvalidCodingException()

		self._write_primative_buffer(struct.pack("!d", float(value)))

	def encode_required_int8(self, value, properties):
		if type(value) not in (int, long):
			raise PinchInvalidCodingException()

		self._write_primative_unsigned_ordinal(value)

	def encode_required_int16(self, value, properties):
		if type(value) not in (int, long):
			raise PinchInvalidCodingException()

		self._write_primative_signed_ordinal(value)

	def encode_required_int32(self, value, properties):
		if type(value) not in (int, long):
			raise PinchInvalidCodingException()

		self._write_primative_signed_ordinal(value)

	def encode_required_int64(self, value, properties):
		if type(value) not in (int, long):
			raise PinchInvalidCodingException()

		self._write_primative_long_ordinal(value)

	def encode_required_decimal(self, value, properties):
		if not isinstance(value, PinchDecimal):
			raise PinchInvalidCodingException()

		self._write_decimal(value)

	def encode_required_bool(self, value, properties):
		if value:
			self._write_primative_unsigned_ordinal(1)
		else:
			self._write_primative_unsigned_ordinal(0)

	def encode_required_string(self, value, properties):
		if type(value) is str:
			value = unicode(value).encode("utf-8")
		elif type(value) is unicode:
			value = value.encode("utf-8")
		else:
			raise PinchInvalidCodingException()

		self._write_primative_buffer(value)

	def encode_required_bytes(self, value, properties):
		if type(value) is not str:
			raise PinchInvalidCodingException()

		self._write_primative_buffer(value)

	def encode_required_enumeration(self, value, properties):
		if type(value) not in (int, long):
			raise PinchInvalidCodingException()

		self._write_primative_unsigned_ordinal(value)

	def encode_required_structure(self, value, properties):
		value.encode(self)

	def encode_optional_float32(self, value, properties):
		if value is not None:
			if type(value) not in (int, long, float):
				raise PinchInvalidCodingException()

			self._write_primative_buffer(struct.pack("!f", float(value)))
		else:
			self._write_null()

	def encode_optional_float64(self, value, properties):
		if value is not None:
			if type(value) not in (int, long, float):
				raise PinchInvalidCodingException()

			self._write_primative_buffer(struct.pack("!d", float(value)))
		else:
			self._write_null()

	def encode_optional_int8(self, value, properties):
		if value is not None:
			if type(value) not in (int, long):
				raise PinchInvalidCodingException()

			self._write_primative_unsigned_ordinal(value)
		else:
			self._write_null()

	def encode_optional_int16(self, value, properties):
		if value is not None:
			if type(value) not in (int, long):
				raise PinchInvalidCodingException()

			self._write_primative_signed_ordinal(value)
		else:
			self._write_null()

	def encode_optional_int32(self, value, properties):
		if value is not None:
			if type(value) not in (int, long):
				raise PinchInvalidCodingException()

			self._write_primative_signed_ordinal(value)
		else:
			self._write_null()

	def encode_optional_int64(self, value, properties):
		if value is not None:
			if type(value) not in (int, long):
				raise PinchInvalidCodingException()

			self._write_primative_long_ordinal(value)
		else:
			self._write_null()

	def encode_optional_decimal(self, value, properties):
		if value is not None:
			if not isinstance(value, PinchDecimal):
				raise PinchInvalidCodingException()

			self._write_decimal(value)
		else:
			self._write_null()

	def encode_optional_bool(self, value, properties):
		if value is not None:
			if value:
				self._write_primative_unsigned_ordinal(1)
			else:
				self._write_primative_unsigned_ordinal(0)
		else:
			self._write_null()

	def encode_optional_string(self, value, properties):
		if value is not None:
			if type(value) is str:
				value = unicode(value).encode("utf-8")
			elif type(value) is unicode:
				value = value.encode("utf-8")
			else:
				raise PinchInvalidCodingException()

			self._write_primative_buffer(value)
		else:
			self._write_null()

	def encode_optional_bytes(self, value, properties):
		if value is not None:
			if type(value) is not str:
				raise PinchInvalidCodingException()

			self._write_primative_buffer(value)
		else:
			self._write_null()

	def encode_optional_enumeration(self, value, properties):
		if value is not None:
			if type(value) not in (int, long):
				raise PinchInvalidCodingException()

			self._write_primative_unsigned_ordinal(value)
		else:
			self._write_null()

	def encode_optional_structure(self, value, properties):
		if value is not None:
			value.encode(self)
		else:
			self._write_null()

	def close_sequence(self):
		pass

def decode_from_file(factory, file):
	decoder = PinchDecoder(file)

	return decoder.decode_required_structure(factory, None)

def decode(factory, encoded):
	decoder = PinchDecoder(cStringIO.StringIO(encoded))

	return decoder.decode_required_structure(factory, None)

def encode_to_file(value, file):
	encoder = PinchEncoder(file)

	return encoder.encode_required_structure(value, None)

def encode(value):
	encoder = PinchEncoder(cStringIO.StringIO())

	encoder.encode_required_structure(value, None)

	return encoder._file.getvalue()

