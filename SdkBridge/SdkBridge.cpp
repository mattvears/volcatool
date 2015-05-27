// SdkBridge.cpp : Defines the exported functions for the DLL application.
#include "stdafx.h"
#include "korg_syro_volcasample.h"
#include "stdio.h"
#include "stdlib.h"

#ifdef NATIVEDLL_EXPORTS
#define NATIVEDLL_API __declspec(dllexport)
#else
#define NATIVEDLL_API __declspec(dllimport)
#endif

#define WAVFMT_POS_ENCODE	0x00
#define WAVFMT_POS_CHANNEL	0x02
#define WAVFMT_POS_FS		0x04
#define WAVFMT_POS_BIT		0x0E

#define WAV_POS_RIFF_SIZE	0x04
#define WAV_POS_WAVEFMT		0x08
#define WAV_POS_DATA_SIZE	0x28

SyroData *syro_data;
unsigned int sample_count = 0;
unsigned int sample_number;

/** string structure to marshall char*'s out of managed code. **/
typedef struct _STRSTRUCT
{
	char* Data;
	UINT Size;
} STRSTRUCT;

static const uint8_t wav_header[] = {
	'R', 'I', 'F', 'F',		// 'RIFF'
	0x00, 0x00, 0x00, 0x00,		// Size (data size + 0x24)
	'W', 'A', 'V', 'E',		// 'WAVE'
	'f', 'm', 't', ' ',		// 'fmt '
	0x10, 0x00, 0x00, 0x00,		// Fmt chunk size
	0x01, 0x00,					// encode(wav)
	0x02, 0x00,					// channel = 2
	0x44, 0xAC, 0x00, 0x00,		// Fs (44.1kHz)
	0x10, 0xB1, 0x02, 0x00,		// Bytes per sec (Fs * 4)
	0x04, 0x00,					// Block Align (2ch,16Bit -> 4)
	0x10, 0x00,					// 16Bit
	'd', 'a', 't', 'a',		// 'data'
	0x00, 0x00, 0x00, 0x00		// data size(bytes)
};

extern "C" NATIVEDLL_API unsigned int __cdecl Convert(STRSTRUCT* newFileName);
extern "C" NATIVEDLL_API unsigned int __cdecl Prepare(unsigned int dataType, unsigned int totalSamples, unsigned int startingSampleNumber, STRSTRUCT* fileName);

/*----------------------------------------------------------------------------
Write 32Bit Value
----------------------------------------------------------------------------*/
static void set_32Bit_value(uint8_t *ptr, uint32_t dat)
{
	int i;

	for (i = 0; i<4; i++) {
		*ptr++ = (uint8_t)dat;
		dat >>= 8;
	}
}

/*----------------------------------------------------------------------------
Read 32Bit Value
----------------------------------------------------------------------------*/
static uint32_t get_32Bit_value(uint8_t *ptr)
{
	int i;
	uint32_t dat;

	dat = 0;

	for (i = 0; i<4; i++) {
		dat <<= 8;
		dat |= (uint32_t)ptr[3 - i];
	}
	return dat;
}

/*----------------------------------------------------------------------------
Read 16Bit Value
----------------------------------------------------------------------------*/
static uint16_t get_16Bit_value(uint8_t *ptr)
{
	uint16_t dat;

	dat = (uint16_t)ptr[1];
	dat <<= 8;
	dat |= (uint16_t)ptr[0];

	return dat;
}
/*----------------------------------------------------------------------------
free data memory
----------------------------------------------------------------------------*/
static void free_syrodata(SyroData *syro_data, int num_of_data)
{
	int i;

	for (i = 0; i<num_of_data; i++) {
		if (syro_data->pData) {
			free(syro_data->pData);
			syro_data->pData = NULL;
		}
		syro_data++;
	}
}

static uint8_t *read_file(char *filename, uint32_t *psize)
{
	FILE *fp;
	uint8_t *buf;
	uint32_t size;

	fp = fopen((const char *)filename, "rb");
	if (!fp) {
		return NULL;
	}

	fseek(fp, 0, SEEK_END);
	size = ftell(fp);
	fseek(fp, 0, SEEK_SET);

	buf = (uint8_t*) malloc(size);
	if (!buf) {
		fclose(fp);
		return NULL;
	}

	if (fread(buf, 1, size, fp) < size) {
		fclose(fp);
		free(buf);
		return NULL;
	}

	fclose(fp);

	*psize = size;	
	return buf;
}

/*----------------------------------------------------------------------------
Write File
----------------------------------------------------------------------------*/
static bool write_file(char *filename, uint8_t *buf, uint32_t size)
{
	FILE *fp;

	fp = fopen(filename, "wb");
	if (!fp) {
		return false;
	}

	if (fwrite(buf, 1, size, fp) < size) {
		fclose(fp);
		return false;
	}

	fclose(fp);

	return true;
}

unsigned int Prepare(unsigned int dataType, unsigned int totalSamples, unsigned int startingSampleNumber, STRSTRUCT* fileName)
{
	uint8_t *src;
	uint32_t wav_pos, size, chunk_size;
	uint32_t wav_fs;
	uint16_t num_of_ch, sample_byte;
	uint32_t num_of_frame;

	if (totalSamples + startingSampleNumber > 99)
	{
		return 3; /** sample slot number exceeds available slot numbers **/
	}

	if (!syro_data)
	{
		syro_data = new SyroData[totalSamples];
		sample_number = startingSampleNumber;
	}

	if (sample_count > totalSamples)
	{
		return 4; /** sample count is out of range **/
	}

	char* fn = fileName->Data;
	src = read_file(fn, &size); 

	//------- check header/fmt -------*/
	if (memcmp(src, wav_header, 4)) {
		free(src);
		return 2; /** illegal data **/
	}

	if (memcmp((src + WAV_POS_WAVEFMT), (wav_header + WAV_POS_WAVEFMT), 8)) {
		free(src);
		return 2; /** illegal data **/
	}

	wav_pos = WAV_POS_WAVEFMT + 4;		// 'fmt ' pos

	if (get_16Bit_value(src + wav_pos + 8 + WAVFMT_POS_ENCODE) != 1) {
		free(src);
		return 2; /** illegal data **/
	}

	num_of_ch = get_16Bit_value(src + wav_pos + 8 + WAVFMT_POS_CHANNEL);
	if ((num_of_ch != 1) && (num_of_ch != 2)) {
		free(src);
		return 2; /** illegal data **/
	}
	
	uint16_t num_of_bit;

	num_of_bit = get_16Bit_value(src + wav_pos + 8 + WAVFMT_POS_BIT);
	if ((num_of_bit != 16) && (num_of_bit != 24)) {
		free(src);
		return 2; /** illegal data **/
	}

	sample_byte = (num_of_bit / 8);

	wav_fs = get_32Bit_value(src + wav_pos + 8 + WAVFMT_POS_FS);

	//------- search 'data' -------*/
	for (;;) {
		chunk_size = get_32Bit_value(src + wav_pos + 4);
		if (!memcmp((src + wav_pos), "data", 4)) {
			break;
		}
		wav_pos += chunk_size + 8;
		if ((wav_pos + 8) > size) {
			free(src);
			return false;
		}
	}

	if ((wav_pos + chunk_size + 8) > size) {
		free(src);
		return false;
	}

	num_of_frame = chunk_size / (num_of_ch * sample_byte);
	chunk_size = (num_of_frame * 2);
	
	syro_data->DataType = (SyroDataType)dataType;
	syro_data->pData = (uint8_t*)malloc(chunk_size);
	
	if (!syro_data->pData) {
		free(src);
		return false;
	}

	//------- convert to 1ch, 16Bit  -------*/
	uint8_t *poss;
	int16_t *posd;
	int32_t dat, datf;
	uint16_t ch, sbyte;

	poss = (src + wav_pos + 8);
	posd = (int16_t *)syro_data->pData;

	for (;;) {
		datf = 0;
		for (ch = 0; ch<num_of_ch; ch++) {
			dat = ((int8_t *)poss)[sample_byte - 1];
			for (sbyte = 1; sbyte<sample_byte; sbyte++) {
				dat <<= 8;
				dat |= poss[sample_byte - 1 - sbyte];
			}
			poss += sample_byte;
			datf += dat;
		}
		datf /= num_of_ch;
		*posd++ = (int16_t)datf;
		if (!(--num_of_frame)) {
			break;
		}
	}

	syro_data->Number = sample_number;
	syro_data->Size = chunk_size;
	syro_data->Fs = wav_fs;
	syro_data->SampleEndian = LittleEndian;
	syro_data->Quality = 16;

	syro_data++;
	sample_count += 1;
	sample_number++;

	free(src);
	return 0;
}

unsigned int Convert(STRSTRUCT* newFileName)
{
	SyroStatus status;
	SyroHandle handle;
	uint8_t *buf_dest;
	uint32_t size_dest;
	uint32_t frame, write_pos;
	int16_t left, right;

	for (int i = sample_count; i > 0; i--)
	{
		syro_data--;
	}

	//----- Start ------
	status = SyroVolcaSample_Start(&handle, syro_data, sample_count, 0, &frame);
	if (status != Status_Success) {
		free_syrodata(syro_data, sample_count);
		return 1;
	}

	size_dest = (frame * 4) + sizeof(wav_header);

	buf_dest = (uint8_t*) malloc(size_dest);
	if (!buf_dest) {
		SyroVolcaSample_End(handle);
		free_syrodata(syro_data, sample_count);
		return 1;
	}

	memcpy(buf_dest, wav_header, sizeof(wav_header));
	set_32Bit_value((buf_dest + WAV_POS_RIFF_SIZE), ((frame * 4) + 0x24));
	set_32Bit_value((buf_dest + WAV_POS_DATA_SIZE), (frame * 4));

	//----- convert loop ------
	write_pos = sizeof(wav_header);
	while (frame) {
		SyroVolcaSample_GetSample(handle, &left, &right);
		buf_dest[write_pos++] = (uint8_t)left;
		buf_dest[write_pos++] = (uint8_t)(left >> 8);
		buf_dest[write_pos++] = (uint8_t)right;
		buf_dest[write_pos++] = (uint8_t)(right >> 8);
		frame--;
	}

	SyroVolcaSample_End(handle);
	free_syrodata(syro_data, sample_count);

	//----- write ------
	char* fn = newFileName->Data;
	write_file(fn, buf_dest, size_dest);

	free(buf_dest);

	return status;
}