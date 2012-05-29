import unittest
import pinch
from testversion3 import *

types_encoded = \
	"\xda\x00\x00\x00\x00\x00\x00\x00\x00\x00\x00\xc1\x00\x00\x00A\x83" \
	"\x01\x02\x03\x83\x00\x00\x01B\x84?\x8c\xcc\xcd\x88?\xf1\x99\x99\x99" \
	"\x99\x99\x9a\x01\x01\x01\xfe\xff\xff\xff\x0f\x01\x01L\xc1\xc1@\x94" \
	"The quick brown fox.\xc1B"

class TestDecodingAndEncoding(unittest.TestCase):
	def test_decoding_structure(self):
		after = pinch.decode(TypesStructureFactory, types_encoded)

		self.assertTrue(abs(after.req_float32 - 1.1) < 0.00001)
		self.assertTrue(abs(after.req_float64 - 1.1) < 0.00001)
		self.assertEqual(after.req_int8, 12)
		self.assertEqual(after.req_int16, -1)
		self.assertEqual(after.req_int32, 0x7fffffff)
		self.assertEqual(after.req_int64, -1L)
		self.assertEqual(after.req_decimal, pinch.PinchDecimal(False, 1, 0))
		self.assertEqual(after.req_bool, True)
		self.assertEqual(after.req_string, u"The quick brown fox.")
		self.assertEqual(after.req_bytes, "\x01\x02\x03")
		self.assertEqual(after.req_enumeration, TypesEnumeration.B)
		self.assertIsInstance(after.req_structure, SmallStructure)
		self.assertEqual(after.req_structure.test, 2)
		self.assertEqual(len(after.req_list_of_enum), 1)
		self.assertIsInstance(after.req_list_of_enum[0], SmallStructure)
		self.assertIsNone(after.opt_float32)
		self.assertIsNone(after.opt_float64)
		self.assertIsNone(after.opt_int8)
		self.assertIsNone(after.opt_int16)
		self.assertIsNone(after.opt_int32)
		self.assertIsNone(after.opt_int64)
		self.assertIsNone(after.opt_decimal)
		self.assertIsNone(after.opt_bool)
		self.assertIsNone(after.opt_string)
		self.assertIsNone(after.opt_bytes)
		self.assertIsNone(after.opt_enumeration)
		self.assertIsNone(after.opt_structure)
		self.assertEqual(len(after.opt_list_of_enum), 1)
		self.assertIsNone(after.opt_list_of_enum[0])

		self.assertEqual(pinch.encode(after), types_encoded)

if __name__ == "__main__":
	unittest.main()

