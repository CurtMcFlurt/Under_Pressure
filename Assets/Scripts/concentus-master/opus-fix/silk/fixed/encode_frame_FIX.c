/***********************************************************************
Copyright (c) 2006-2011, Skype Limited. All rights reserved.
Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions
are met:
- Redistributions of source code must retain the above copyright notice,
this list of conditions and the following disclaimer.
- Redistributions in binary form must reproduce the above copyright
notice, this list of conditions and the following disclaimer in the
documentation and/or other materials provided with the distribution.
- Neither the name of Internet Society, IETF or IETF Trust, nor the
names of specific contributors, may be used to endorse or promote
products derived from this software without specific prior written
permission.
THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
POSSIBILITY OF SUCH DAMAGE.
***********************************************************************/

#ifdef HAVE_CONFIG_H
#include "config.h"
#endif

#include "main_FIX.h"
#include "stack_alloc.h"
#include "tuning_parameters.h"

#define TRACE_FILE 0
#if TRACE_FILE
#include "NailTester.h"
#else
void NailTesterPrint_silk_encoder_state_FIX(char* var1, void* var2) {}
#endif

/* Low Bitrate Redundancy (LBRR) encoding. Reuse all parameters but encode with lower bitrate           */
static OPUS_INLINE void silk_LBRR_encode_FIX(
    silk_encoder_state_FIX          *psEnc,                                 /* I/O  Pointer to Silk FIX encoder state                                           */
    silk_encoder_control_FIX        *psEncCtrl,                             /* I/O  Pointer to Silk FIX encoder control struct                                  */
    const opus_int32                xfw_Q3[],                               /* I    Input signal                                                                */
    opus_int                        condCoding                              /* I    The type of conditional coding used so far for this frame                   */
);

void silk_encode_do_VAD_FIX(
    silk_encoder_state_FIX          *psEnc                                  /* I/O  Pointer to Silk FIX encoder state                                           */
)
{
    /****************************/
    /* Voice Activity Detection */
    /****************************/
    silk_VAD_GetSA_Q8( &psEnc->sCmn, psEnc->sCmn.inputBuf + 1, psEnc->sCmn.arch );

    /**************************************************/
    /* Convert speech activity into VAD and DTX flags */
    /**************************************************/
    if( psEnc->sCmn.speech_activity_Q8 < SILK_FIX_CONST( SPEECH_ACTIVITY_DTX_THRES, 8 ) ) {
        psEnc->sCmn.indices.signalType = TYPE_NO_VOICE_ACTIVITY;
        psEnc->sCmn.noSpeechCounter++;
        if( psEnc->sCmn.noSpeechCounter < NB_SPEECH_FRAMES_BEFORE_DTX ) {
            psEnc->sCmn.inDTX = 0;
        } else if( psEnc->sCmn.noSpeechCounter > MAX_CONSECUTIVE_DTX + NB_SPEECH_FRAMES_BEFORE_DTX ) {
            psEnc->sCmn.noSpeechCounter = NB_SPEECH_FRAMES_BEFORE_DTX;
            psEnc->sCmn.inDTX           = 0;
        }
        psEnc->sCmn.VAD_flags[ psEnc->sCmn.nFramesEncoded ] = 0;
    } else {
        psEnc->sCmn.noSpeechCounter    = 0;
        psEnc->sCmn.inDTX              = 0;
        psEnc->sCmn.indices.signalType = TYPE_UNVOICED;
        psEnc->sCmn.VAD_flags[ psEnc->sCmn.nFramesEncoded ] = 1;
    }
}

static opus_int TEST_COUNT = 0;
static opus_int TARGET_TEST = 1;

/****************/
/* Encode frame */
/****************/
opus_int silk_encode_frame_FIX(
    silk_encoder_state_FIX          *psEnc,                                 /* I/O  Pointer to Silk FIX encoder state                                           */
    opus_int32                      *pnBytesOut,                            /* O    Pointer to number of payload bytes;                                         */
    ec_enc                          *psRangeEnc,                            /* I/O  compressor data structure                                                   */
    opus_int                        condCoding,                             /* I    The type of conditional coding to use                                       */
    opus_int                        maxBits,                                /* I    If > 0: maximum number of output bits                                       */
    opus_int                        useCBR                                  /* I    Flag to force constant-bitrate operation                                    */
)
{
	if (TRACE_FILE) printf("Entering silk encode frame\n");
	if (TRACE_FILE) NailTesterPrint_silk_encoder_state_FIX("state1", psEnc);
	/*NailTestPrintTestHeader("silk_encode_frame_FIX_manual");
	fprintf(stdout, "#region autogen\n");
	fprintf(stdout, "silk_encoder_state_fix ");
	NailTesterPrint_silk_encoder_state_FIX("through_psEnc", psEnc);
	fprintf(stdout, "ec_ctx ");
	NailTesterPrint_ec_ctx("through_psRangeEnc", psRangeEnc);
	NailTestPrintInputIntDeclaration("condCoding", condCoding);
	NailTestPrintInputIntDeclaration("maxBits", maxBits);
	NailTestPrintInputIntDeclaration("useCBR", useCBR);*/

	/*if (TEST_COUNT == TARGET_TEST)
	{
		fprintf(stdout, "Test begin------------------------------------------------\n");
	}*/

	silk_encoder_control_FIX sEncCtrl;
    opus_int     i, iter, maxIter, found_upper, found_lower, ret = 0;
    opus_int16   *x_frame;
    ec_enc       sRangeEnc_copy, sRangeEnc_copy2;
    silk_nsq_state sNSQ_copy, sNSQ_copy2;
    opus_int32   seed_copy, nBits, nBits_lower, nBits_upper, gainMult_lower, gainMult_upper;
    opus_int32   gainsID, gainsID_lower, gainsID_upper;
    opus_int16   gainMult_Q8;
    opus_int16   ec_prevLagIndex_copy;
    opus_int     ec_prevSignalType_copy;
    opus_int8    LastGainIndex_copy2;
    SAVE_STACK;

    /* This is totally unnecessary but many compilers (including gcc) are too dumb to realise it */
    LastGainIndex_copy2 = nBits_lower = nBits_upper = gainMult_lower = gainMult_upper = 0;

    psEnc->sCmn.indices.Seed = psEnc->sCmn.frameCounter++ & 3;

    /**************************************************************/
    /* Set up Input Pointers, and insert frame in input buffer   */
    /*************************************************************/
    /* start of frame to encode */
    x_frame = psEnc->x_buf + psEnc->sCmn.ltp_mem_length;

    /***************************************/
    /* Ensure smooth bandwidth transitions */
    /***************************************/
    silk_LP_variable_cutoff( &psEnc->sCmn.sLP, psEnc->sCmn.inputBuf + 1, psEnc->sCmn.frame_length );

    /*******************************************/
    /* Copy new frame to front of input buffer */
    /*******************************************/
    silk_memcpy( x_frame + LA_SHAPE_MS * psEnc->sCmn.fs_kHz, psEnc->sCmn.inputBuf + 1, psEnc->sCmn.frame_length * sizeof( opus_int16 ) );

    if( !psEnc->sCmn.prefillFlag ) {
        VARDECL( opus_int32, xfw_Q3 );
        VARDECL( opus_int16, res_pitch );
        VARDECL( opus_uint8, ec_buf_copy );
        opus_int16 *res_pitch_frame;

        ALLOC( res_pitch,
               psEnc->sCmn.la_pitch + psEnc->sCmn.frame_length
                   + psEnc->sCmn.ltp_mem_length, opus_int16 );
        /* start of pitch LPC residual frame */
        res_pitch_frame = res_pitch + psEnc->sCmn.ltp_mem_length;

		/*NailTestPrintTestHeader("silk_encode_frame_internal");
		fprintf(stdout, "#region autogen\n");
		fprintf(stdout, "silk_encoder_state_fix ");
		NailTesterPrint_silk_encoder_state_FIX("through_psEnc", psEnc);
		fprintf(stdout, "silk_encoder_control ");
		NailTesterPrint_silk_encoder_control("through_sEncCtrl", &sEncCtrl);
		NailTestPrintInputShortArrayDeclaration("x_frame", x_frame - 500, 2000);
		fprintf(stdout, "in_x_frame = in_x_frame.Point(500);\n");
		fprintf(stdout, "#endregion\n");
		NailTestPrintInputIntDeclaration("condCoding", condCoding);

		fprintf(stdout, "Pointer<short> res_pitch = Pointer.Malloc<short>(through_psEnc.sCmn.la_pitch + through_psEnc.sCmn.frame_length + through_psEnc.sCmn.ltp_mem_length);\n");
		fprintf(stdout, "Pointer<short> res_pitch_frame = res_pitch.Point(through_psEnc.sCmn.ltp_mem_length);\n");
		*/
		/*****************************************/
		/* Find pitch lags, initial LPC analysis */
		/*****************************************/
		silk_find_pitch_lags_FIX(psEnc, &sEncCtrl, res_pitch, x_frame, psEnc->sCmn.arch);

		if (TRACE_FILE) NailTesterPrint_silk_encoder_state_FIX("state2", psEnc);

		/*fprintf(stdout, "find_pitch_lags.silk_find_pitch_lags_FIX(through_psEnc, through_sEncCtrl, res_pitch, in_x_frame, through_psEnc.sCmn.arch);\n");

		fprintf(stdout, "#region autogen\n");
		fprintf(stdout, "silk_encoder_state_fix ");
		NailTesterPrint_silk_encoder_state_FIX("expectedEncoder", psEnc);
		fprintf(stdout, "silk_encoder_control ");
		NailTesterPrint_silk_encoder_control("expectedControl", &sEncCtrl);
		fprintf(stdout, "#endregion\n");
		fprintf(stdout, "Helpers.AssertSilkEncStateEquals(expectedEncoder, through_psEnc);\n");
		fprintf(stdout, "Helpers.AssertSilkEncControlEquals(expectedControl, through_sEncCtrl);\n");
		*/
		/************************/
		/* Noise shape analysis */
		/************************/
		silk_noise_shape_analysis_FIX(psEnc, &sEncCtrl, res_pitch_frame, x_frame, psEnc->sCmn.arch);

		if (TRACE_FILE) NailTesterPrint_silk_encoder_state_FIX("state3", psEnc);

		/*fprintf(stdout, "noise_shape_analysis.silk_noise_shape_analysis_FIX(through_psEnc, through_sEncCtrl, res_pitch_frame, in_x_frame, through_psEnc.sCmn.arch);\n");

		fprintf(stdout, "#region autogen\n");
		NailTesterPrint_silk_encoder_state_FIX("expectedEncoder", psEnc);
		NailTesterPrint_silk_encoder_control("expectedControl", &sEncCtrl);
		fprintf(stdout, "#endregion\n");
		fprintf(stdout, "Helpers.AssertSilkEncStateEquals(expectedEncoder, through_psEnc);\n");
		fprintf(stdout, "Helpers.AssertSilkEncControlEquals(expectedControl, through_sEncCtrl);\n");
		*/
		/***************************************************/
		/* Find linear prediction coefficients (LPC + LTP) */
		/***************************************************/
		silk_find_pred_coefs_FIX(psEnc, &sEncCtrl, res_pitch, x_frame, condCoding);

		if (TRACE_FILE) NailTesterPrint_silk_encoder_state_FIX("state4", psEnc);

		/*fprintf(stdout, "find_pred_coefs.silk_find_pred_coefs_FIX(through_psEnc, through_sEncCtrl, res_pitch, in_x_frame, in_condCoding);\n");

		fprintf(stdout, "#region autogen\n");
		NailTesterPrint_silk_encoder_state_FIX("expectedEncoder", psEnc);
		NailTesterPrint_silk_encoder_control("expectedControl", &sEncCtrl);
		fprintf(stdout, "#endregion\n");
		fprintf(stdout, "Helpers.AssertSilkEncStateEquals(expectedEncoder, through_psEnc);\n");
		fprintf(stdout, "Helpers.AssertSilkEncControlEquals(expectedControl, through_sEncCtrl);\n");
		*/
		/****************************************/
		/* Process gains                        */
		/****************************************/
		silk_process_gains_FIX(psEnc, &sEncCtrl, condCoding);

		if (TRACE_FILE) NailTesterPrint_silk_encoder_state_FIX("state5", psEnc);

		/*fprintf(stdout, "process_gains.silk_process_gains_FIX(through_psEnc, through_sEncCtrl, in_condCoding);\n");

		fprintf(stdout, "#region autogen\n");
		NailTesterPrint_silk_encoder_state_FIX("expectedEncoder", psEnc);
		NailTesterPrint_silk_encoder_control("expectedControl", &sEncCtrl);
		fprintf(stdout, "#endregion\n");
		fprintf(stdout, "Helpers.AssertSilkEncStateEquals(expectedEncoder, through_psEnc);\n");
		fprintf(stdout, "Helpers.AssertSilkEncControlEquals(expectedControl, through_sEncCtrl);\n");
		*/
		/*****************************************/
		/* Prefiltering for noise shaper         */
		/*****************************************/
		ALLOC(xfw_Q3, psEnc->sCmn.frame_length, opus_int32);
		silk_prefilter_FIX(psEnc, &sEncCtrl, xfw_Q3, x_frame);

		if (TRACE_FILE) NailTesterPrint_silk_encoder_state_FIX("state6", psEnc);

		/*fprintf(stdout, "Pointer<int> xfw_Q3 = Pointer.Malloc<int>(through_psEnc.sCmn.frame_length);\n");
		fprintf(stdout, "prefilter.silk_prefilter_FIX(through_psEnc, through_sEncCtrl, xfw_Q3, in_x_frame);\n");

		fprintf(stdout, "#region autogen\n");
		NailTestPrintOutputIntArrayDeclaration("xfw_Q3_1", xfw_Q3, psEnc->sCmn.frame_length);
		NailTesterPrint_silk_encoder_state_FIX("expectedEncoder", psEnc);
		NailTesterPrint_silk_encoder_control("expectedControl", &sEncCtrl);
		fprintf(stdout, "#endregion\n");
		fprintf(stdout, "Helpers.AssertSilkEncStateEquals(expectedEncoder, through_psEnc);\n");
		fprintf(stdout, "Helpers.AssertSilkEncControlEquals(expectedControl, through_sEncCtrl);\n");
		fprintf(stdout, "Helpers.AssertArrayDataEquals(expected_xfw_Q3_1, xfw_Q3);\n");
		*/
		/****************************************/
		/* Low Bitrate Redundant Encoding       */
		/****************************************/
		silk_LBRR_encode_FIX(psEnc, &sEncCtrl, xfw_Q3, condCoding);

		if (TRACE_FILE) NailTesterPrint_silk_encoder_state_FIX("state7", psEnc);

		/*fprintf(stdout, "encode_frame.silk_LBRR_encode_FIX(through_psEnc, through_sEncCtrl, xfw_Q3, in_condCoding);\n");

		fprintf(stdout, "#region autogen\n");
		NailTestPrintOutputIntArrayDeclaration("xfw_Q3_2", xfw_Q3, psEnc->sCmn.frame_length);
		NailTesterPrint_silk_encoder_state_FIX("expectedEncoder", psEnc);
		NailTesterPrint_silk_encoder_control("expectedControl", &sEncCtrl);
		fprintf(stdout, "#endregion\n");
		fprintf(stdout, "Helpers.AssertSilkEncStateEquals(expectedEncoder, through_psEnc);\n");
		fprintf(stdout, "Helpers.AssertSilkEncControlEquals(expectedControl, through_sEncCtrl);\n");
		fprintf(stdout, "Helpers.AssertArrayDataEquals(expected_xfw_Q3_2, xfw_Q3);\n");
		NailTestPrintTestFooter();*/
		
        /* Loop over quantizer and entropy coding to control bitrate */
        maxIter = 6;
        gainMult_Q8 = SILK_FIX_CONST( 1, 8 );
        found_lower = 0;
        found_upper = 0;
        gainsID = silk_gains_ID( psEnc->sCmn.indices.GainsIndices, psEnc->sCmn.nb_subfr );
        gainsID_lower = -1;
        gainsID_upper = -1;
        /* Copy part of the input state */
        silk_memcpy( &sRangeEnc_copy, psRangeEnc, sizeof( ec_enc ) );
        silk_memcpy( &sNSQ_copy, &psEnc->sCmn.sNSQ, sizeof( silk_nsq_state ) );
        seed_copy = psEnc->sCmn.indices.Seed;
        ec_prevLagIndex_copy = psEnc->sCmn.ec_prevLagIndex;
        ec_prevSignalType_copy = psEnc->sCmn.ec_prevSignalType;
        ALLOC( ec_buf_copy, 1275, opus_uint8 );
        for( iter = 0; ; iter++ ) {
            if( gainsID == gainsID_lower ) {
                nBits = nBits_lower;
            } else if( gainsID == gainsID_upper ) {
                nBits = nBits_upper;
            } else {
                /* Restore part of the input state */
                if( iter > 0 ) {
                    silk_memcpy( psRangeEnc, &sRangeEnc_copy, sizeof( ec_enc ) );
                    silk_memcpy( &psEnc->sCmn.sNSQ, &sNSQ_copy, sizeof( silk_nsq_state ) );
                    psEnc->sCmn.indices.Seed = seed_copy;
                    psEnc->sCmn.ec_prevLagIndex = ec_prevLagIndex_copy;
                    psEnc->sCmn.ec_prevSignalType = ec_prevSignalType_copy;
                }

                /*****************************************/
                /* Noise shaping quantization            */
                /*****************************************/
                if( psEnc->sCmn.nStatesDelayedDecision > 1 || psEnc->sCmn.warping_Q16 > 0 ) {
                    silk_NSQ_del_dec( &psEnc->sCmn, &psEnc->sCmn.sNSQ, &psEnc->sCmn.indices, xfw_Q3, psEnc->sCmn.pulses,
                           sEncCtrl.PredCoef_Q12[ 0 ], sEncCtrl.LTPCoef_Q14, sEncCtrl.AR2_Q13, sEncCtrl.HarmShapeGain_Q14,
                           sEncCtrl.Tilt_Q14, sEncCtrl.LF_shp_Q14, sEncCtrl.Gains_Q16, sEncCtrl.pitchL, sEncCtrl.Lambda_Q10, sEncCtrl.LTP_scale_Q14,
                           psEnc->sCmn.arch );
                } else {
                    silk_NSQ( &psEnc->sCmn, &psEnc->sCmn.sNSQ, &psEnc->sCmn.indices, xfw_Q3, psEnc->sCmn.pulses,
                            sEncCtrl.PredCoef_Q12[ 0 ], sEncCtrl.LTPCoef_Q14, sEncCtrl.AR2_Q13, sEncCtrl.HarmShapeGain_Q14,
                            sEncCtrl.Tilt_Q14, sEncCtrl.LF_shp_Q14, sEncCtrl.Gains_Q16, sEncCtrl.pitchL, sEncCtrl.Lambda_Q10, sEncCtrl.LTP_scale_Q14,
                            psEnc->sCmn.arch);
                }

				if (TRACE_FILE) NailTesterPrint_silk_encoder_state_FIX("state8", psEnc);

                /****************************************/
                /* Encode Parameters                    */
                /****************************************/
                silk_encode_indices( &psEnc->sCmn, psRangeEnc, psEnc->sCmn.nFramesEncoded, 0, condCoding );

				if (TRACE_FILE) NailTesterPrint_silk_encoder_state_FIX("state9", psEnc);

                /****************************************/
                /* Encode Excitation Signal             */
                /****************************************/
                silk_encode_pulses( psRangeEnc, psEnc->sCmn.indices.signalType, psEnc->sCmn.indices.quantOffsetType,
                    psEnc->sCmn.pulses, psEnc->sCmn.frame_length );

				if (TRACE_FILE) NailTesterPrint_silk_encoder_state_FIX("state10", psEnc);

                nBits = ec_tell( psRangeEnc );

                if( useCBR == 0 && iter == 0 && nBits <= maxBits ) {
                    break;
                }
            }

            if( iter == maxIter ) {
                if( found_lower && ( gainsID == gainsID_lower || nBits > maxBits ) ) {
                    /* Restore output state from earlier iteration that did meet the bitrate budget */
                    silk_memcpy( psRangeEnc, &sRangeEnc_copy2, sizeof( ec_enc ) );
                    silk_assert( sRangeEnc_copy2.offs <= 1275 );
                    silk_memcpy( psRangeEnc->buf, ec_buf_copy, sRangeEnc_copy2.offs );
                    silk_memcpy( &psEnc->sCmn.sNSQ, &sNSQ_copy2, sizeof( silk_nsq_state ) );
                    psEnc->sShape.LastGainIndex = LastGainIndex_copy2;
                }
                break;
            }

            if( nBits > maxBits ) {
                if( found_lower == 0 && iter >= 2 ) {
                    /* Adjust the quantizer's rate/distortion tradeoff and discard previous "upper" results */
                    sEncCtrl.Lambda_Q10 = silk_ADD_RSHIFT32( sEncCtrl.Lambda_Q10, sEncCtrl.Lambda_Q10, 1 );
                    found_upper = 0;
                    gainsID_upper = -1;
                } else {
                    found_upper = 1;
                    nBits_upper = nBits;
                    gainMult_upper = gainMult_Q8;
                    gainsID_upper = gainsID;
                }
            } else if( nBits < maxBits - 5 ) {
                found_lower = 1;
                nBits_lower = nBits;
                gainMult_lower = gainMult_Q8;
                if( gainsID != gainsID_lower ) {
                    gainsID_lower = gainsID;
                    /* Copy part of the output state */
                    silk_memcpy( &sRangeEnc_copy2, psRangeEnc, sizeof( ec_enc ) );
                    silk_assert( psRangeEnc->offs <= 1275 );
                    silk_memcpy( ec_buf_copy, psRangeEnc->buf, psRangeEnc->offs );
                    silk_memcpy( &sNSQ_copy2, &psEnc->sCmn.sNSQ, sizeof( silk_nsq_state ) );
                    LastGainIndex_copy2 = psEnc->sShape.LastGainIndex;
                }
            } else {
                /* Within 5 bits of budget: close enough */
                break;
            }

            if( ( found_lower & found_upper ) == 0 ) {
                /* Adjust gain according to high-rate rate/distortion curve */
                opus_int32 gain_factor_Q16;
                gain_factor_Q16 = silk_log2lin( silk_LSHIFT( nBits - maxBits, 7 ) / psEnc->sCmn.frame_length + SILK_FIX_CONST( 16, 7 ) );
				gain_factor_Q16 = silk_min_32( gain_factor_Q16, SILK_FIX_CONST( 2, 16 ) );
				if( nBits > maxBits ) {
                    gain_factor_Q16 = silk_max_32( gain_factor_Q16, SILK_FIX_CONST( 1.3, 16 ) );
					 }
                gainMult_Q8 = silk_SMULWB( gain_factor_Q16, gainMult_Q8 );
            } else {
                /* Adjust gain by interpolating */
                gainMult_Q8 = gainMult_lower + silk_DIV32_16( silk_MUL( gainMult_upper - gainMult_lower, maxBits - nBits_lower ), nBits_upper - nBits_lower );
				/* New gain multplier must be between 25% and 75% of old range (note that gainMult_upper < gainMult_lower) */
                if( gainMult_Q8 > silk_ADD_RSHIFT32( gainMult_lower, gainMult_upper - gainMult_lower, 2 ) ) {
                    gainMult_Q8 = silk_ADD_RSHIFT32( gainMult_lower, gainMult_upper - gainMult_lower, 2 );
					} else
                if( gainMult_Q8 < silk_SUB_RSHIFT32( gainMult_upper, gainMult_upper - gainMult_lower, 2 ) ) {
                    gainMult_Q8 = silk_SUB_RSHIFT32( gainMult_upper, gainMult_upper - gainMult_lower, 2 );
					}
            }

            for( i = 0; i < psEnc->sCmn.nb_subfr; i++ ) {
				sEncCtrl.Gains_Q16[ i ] = silk_LSHIFT_SAT32( silk_SMULWB( sEncCtrl.GainsUnq_Q16[ i ], gainMult_Q8 ), 8 );
				}

            /* Quantize gains */
            psEnc->sShape.LastGainIndex = sEncCtrl.lastGainIndexPrev;
            silk_gains_quant( psEnc->sCmn.indices.GainsIndices, sEncCtrl.Gains_Q16,
                  &psEnc->sShape.LastGainIndex, condCoding == CODE_CONDITIONALLY, psEnc->sCmn.nb_subfr );

			if (TRACE_FILE) NailTesterPrint_silk_encoder_state_FIX("state11", psEnc);

            /* Unique identifier of gains vector */
            gainsID = silk_gains_ID( psEnc->sCmn.indices.GainsIndices, psEnc->sCmn.nb_subfr );
        }
    }

    /* Update input buffer */
    silk_memmove( psEnc->x_buf, &psEnc->x_buf[ psEnc->sCmn.frame_length ],
        ( psEnc->sCmn.ltp_mem_length + LA_SHAPE_MS * psEnc->sCmn.fs_kHz ) * sizeof( opus_int16 ) );

    /* Exit without entropy coding */
    if( psEnc->sCmn.prefillFlag ) {
        /* No payload */
        *pnBytesOut = 0;
        RESTORE_STACK;
        return ret;
    }

    /* Parameters needed for next frame */
    psEnc->sCmn.prevLag        = sEncCtrl.pitchL[ psEnc->sCmn.nb_subfr - 1 ];
    psEnc->sCmn.prevSignalType = psEnc->sCmn.indices.signalType;

    /****************************************/
    /* Finalize payload                     */
    /****************************************/
    psEnc->sCmn.first_frame_after_reset = 0;
    /* Payload size */
    *pnBytesOut = silk_RSHIFT( ec_tell( psRangeEnc ) + 7, 3 );

	if (TRACE_FILE) NailTesterPrint_silk_encoder_state_FIX("state12", psEnc);

	if (TRACE_FILE) printf("Exiting silk encode frame\n");

	/*fprintf(stdout, "silk_encoder_state_fix ");
	NailTesterPrint_silk_encoder_state_FIX("expected_psEnc", psEnc);
	fprintf(stdout, "ec_ctx ");
	NailTesterPrint_ec_ctx("expected_psRangeEnc", psRangeEnc);
	fprintf(stdout, "#endregion\n");
	fprintf(stdout, "BoxedValue<int> out_pnBytesOut = new BoxedValue<int>();\n");
	NailTestPrintOutputIntDeclaration("pnBytesOut", *pnBytesOut);
	fprintf(stdout, "int returnVal = encode_frame.silk_encode_frame_FIX(through_psEnc, out_pnBytesOut, through_psRangeEnc, in_condCoding, in_maxBits, in_useCBR);\n");
	fprintf(stdout, "Assert.AreEqual(0, returnVal);\n");
	fprintf(stdout, "Assert.AreEqual(expected_pnBytesOut, out_pnBytesOut.Val);\n");
	fprintf(stdout, "Helpers.AssertEcCtxEquals(expected_psRangeEnc, through_psRangeEnc);\n");
	fprintf(stdout, "Helpers.AssertSilkEncStateEquals(expected_psEnc, through_psEnc);\n");

	NailTestPrintTestFooter();*/

	/*if (TEST_COUNT++ == TARGET_TEST)
	{
		//fprintf(stdout, "Test end\n");
		exit(0);
	}*/

    RESTORE_STACK;

    return ret;
}

/* Low-Bitrate Redundancy (LBRR) encoding. Reuse all parameters but encode excitation at lower bitrate  */
static OPUS_INLINE void silk_LBRR_encode_FIX(
    silk_encoder_state_FIX          *psEnc,                                 /* I/O  Pointer to Silk FIX encoder state                                           */
    silk_encoder_control_FIX        *psEncCtrl,                             /* I/O  Pointer to Silk FIX encoder control struct                                  */
    const opus_int32                xfw_Q3[],                               /* I    Input signal                                                                */
    opus_int                        condCoding                              /* I    The type of conditional coding used so far for this frame                   */
)
{
    opus_int32   TempGains_Q16[ MAX_NB_SUBFR ];
    SideInfoIndices *psIndices_LBRR = &psEnc->sCmn.indices_LBRR[ psEnc->sCmn.nFramesEncoded ];
    silk_nsq_state sNSQ_LBRR;

    /*******************************************/
    /* Control use of inband LBRR              */
    /*******************************************/
    if( psEnc->sCmn.LBRR_enabled && psEnc->sCmn.speech_activity_Q8 > SILK_FIX_CONST( LBRR_SPEECH_ACTIVITY_THRES, 8 ) ) {
        psEnc->sCmn.LBRR_flags[ psEnc->sCmn.nFramesEncoded ] = 1;

        /* Copy noise shaping quantizer state and quantization indices from regular encoding */
        silk_memcpy( &sNSQ_LBRR, &psEnc->sCmn.sNSQ, sizeof( silk_nsq_state ) );
        silk_memcpy( psIndices_LBRR, &psEnc->sCmn.indices, sizeof( SideInfoIndices ) );

        /* Save original gains */
        silk_memcpy( TempGains_Q16, psEncCtrl->Gains_Q16, psEnc->sCmn.nb_subfr * sizeof( opus_int32 ) );

        if( psEnc->sCmn.nFramesEncoded == 0 || psEnc->sCmn.LBRR_flags[ psEnc->sCmn.nFramesEncoded - 1 ] == 0 ) {
            /* First frame in packet or previous frame not LBRR coded */
            psEnc->sCmn.LBRRprevLastGainIndex = psEnc->sShape.LastGainIndex;

            /* Increase Gains to get target LBRR rate */
            psIndices_LBRR->GainsIndices[ 0 ] = psIndices_LBRR->GainsIndices[ 0 ] + psEnc->sCmn.LBRR_GainIncreases;
            psIndices_LBRR->GainsIndices[ 0 ] = silk_min_int( psIndices_LBRR->GainsIndices[ 0 ], N_LEVELS_QGAIN - 1 );
        }

        /* Decode to get gains in sync with decoder         */
        /* Overwrite unquantized gains with quantized gains */
        silk_gains_dequant( psEncCtrl->Gains_Q16, psIndices_LBRR->GainsIndices,
            &psEnc->sCmn.LBRRprevLastGainIndex, condCoding == CODE_CONDITIONALLY, psEnc->sCmn.nb_subfr );

        /*****************************************/
        /* Noise shaping quantization            */
        /*****************************************/
        if( psEnc->sCmn.nStatesDelayedDecision > 1 || psEnc->sCmn.warping_Q16 > 0 ) {
            silk_NSQ_del_dec( &psEnc->sCmn, &sNSQ_LBRR, psIndices_LBRR, xfw_Q3,
                psEnc->sCmn.pulses_LBRR[ psEnc->sCmn.nFramesEncoded ], psEncCtrl->PredCoef_Q12[ 0 ], psEncCtrl->LTPCoef_Q14,
                psEncCtrl->AR2_Q13, psEncCtrl->HarmShapeGain_Q14, psEncCtrl->Tilt_Q14, psEncCtrl->LF_shp_Q14,
                psEncCtrl->Gains_Q16, psEncCtrl->pitchL, psEncCtrl->Lambda_Q10, psEncCtrl->LTP_scale_Q14, psEnc->sCmn.arch );
        } else {
            silk_NSQ( &psEnc->sCmn, &sNSQ_LBRR, psIndices_LBRR, xfw_Q3,
                psEnc->sCmn.pulses_LBRR[ psEnc->sCmn.nFramesEncoded ], psEncCtrl->PredCoef_Q12[ 0 ], psEncCtrl->LTPCoef_Q14,
                psEncCtrl->AR2_Q13, psEncCtrl->HarmShapeGain_Q14, psEncCtrl->Tilt_Q14, psEncCtrl->LF_shp_Q14,
                psEncCtrl->Gains_Q16, psEncCtrl->pitchL, psEncCtrl->Lambda_Q10, psEncCtrl->LTP_scale_Q14, psEnc->sCmn.arch );
        }

        /* Restore original gains */
        silk_memcpy( psEncCtrl->Gains_Q16, TempGains_Q16, psEnc->sCmn.nb_subfr * sizeof( opus_int32 ) );
    }
}
