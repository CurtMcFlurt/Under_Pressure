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

#include "main.h"
#include "stack_alloc.h"
//#include "NailTester.h"

/***********************/
/* NLSF vector encoder */
/***********************/
opus_int32 silk_NLSF_encode(                                    /* O    Returns RD value in Q25                     */
          opus_int8             *NLSFIndices,                   /* I    Codebook path vector [ LPC_ORDER + 1 ]      */
          opus_int16            *pNLSF_Q15,                     /* I/O  Quantized NLSF vector [ LPC_ORDER ]         */
    const silk_NLSF_CB_struct   *psNLSF_CB,                     /* I    Codebook object                             */
    const opus_int16            *pW_QW,                         /* I    NLSF weight vector [ LPC_ORDER ]            */
    const opus_int              NLSF_mu_Q20,                    /* I    Rate weight for the RD optimization         */
    const opus_int              nSurvivors,                     /* I    Max survivors after first stage             */
    const opus_int              signalType                      /* I    Signal type: 0/1/2                          */
)
{
	/*NailTestPrintTestHeader("silk_NLSF_encode");
	fprintf(stdout, "#region autogen\n");
	NailTestPrintInputSbyteArrayDeclaration("NLSFIndices", NLSFIndices, MAX_LPC_ORDER + 1);
	NailTestPrintInputShortArrayDeclaration("pNLSF_Q15", pNLSF_Q15, MAX_LPC_ORDER);
	fprintf(stdout, "silk_NLSF_CB_struct ");
	NailTesterPrint_silk_NLSF_CB_struct("in_psNLSF_CB", psNLSF_CB, 512);
	NailTestPrintInputShortArrayDeclaration("pW_QW", pW_QW, MAX_LPC_ORDER);
	NailTestPrintInputIntDeclaration("NLSF_mu_Q20", NLSF_mu_Q20);
	NailTestPrintInputIntDeclaration("nSurvivors", nSurvivors);
	NailTestPrintInputIntDeclaration("signalType", signalType);*/
	
	opus_int         i, s, ind1, bestIndex, prob_Q8, bits_q7;
    opus_int32       W_tmp_Q9;
    VARDECL( opus_int32, err_Q26 );
    VARDECL( opus_int32, RD_Q25 );
    VARDECL( opus_int, tempIndices1 );
    VARDECL( opus_int8, tempIndices2 );
    opus_int16       res_Q15[      MAX_LPC_ORDER ];
    opus_int16       res_Q10[      MAX_LPC_ORDER ];
    opus_int16       NLSF_tmp_Q15[ MAX_LPC_ORDER ];
    opus_int16       W_tmp_QW[     MAX_LPC_ORDER ];
    opus_int16       W_adj_Q5[     MAX_LPC_ORDER ];
    opus_uint8       pred_Q8[      MAX_LPC_ORDER ];
    opus_int16       ec_ix[        MAX_LPC_ORDER ];
    const opus_uint8 *pCB_element, *iCDF_ptr;
    SAVE_STACK;

    silk_assert( nSurvivors <= NLSF_VQ_MAX_SURVIVORS );
    silk_assert( signalType >= 0 && signalType <= 2 );
    silk_assert( NLSF_mu_Q20 <= 32767 && NLSF_mu_Q20 >= 0 );

    /* NLSF stabilization */
    silk_NLSF_stabilize( pNLSF_Q15, psNLSF_CB->deltaMin_Q15, psNLSF_CB->order );

    /* First stage: VQ */
    ALLOC( err_Q26, psNLSF_CB->nVectors, opus_int32 );
    silk_NLSF_VQ( err_Q26, pNLSF_Q15, psNLSF_CB->CB1_NLSF_Q8, psNLSF_CB->nVectors, psNLSF_CB->order );

    /* Sort the quantization errors */
    ALLOC( tempIndices1, nSurvivors, opus_int );
    silk_insertion_sort_increasing( err_Q26, tempIndices1, psNLSF_CB->nVectors, nSurvivors );

    ALLOC( RD_Q25, nSurvivors, opus_int32 );
    ALLOC( tempIndices2, nSurvivors * MAX_LPC_ORDER, opus_int8 );

    /* Loop over survivors */
    for( s = 0; s < nSurvivors; s++ ) {
        ind1 = tempIndices1[ s ];

        /* Residual after first stage */
        pCB_element = &psNLSF_CB->CB1_NLSF_Q8[ ind1 * psNLSF_CB->order ];
        for( i = 0; i < psNLSF_CB->order; i++ ) {
            NLSF_tmp_Q15[ i ] = silk_LSHIFT16( (opus_int16)pCB_element[ i ], 7 );
            res_Q15[ i ] = pNLSF_Q15[ i ] - NLSF_tmp_Q15[ i ];
        }

        /* Weights from codebook vector */
        silk_NLSF_VQ_weights_laroia( W_tmp_QW, NLSF_tmp_Q15, psNLSF_CB->order );

        /* Apply square-rooted weights */
        for( i = 0; i < psNLSF_CB->order; i++ ) {
            W_tmp_Q9 = silk_SQRT_APPROX( silk_LSHIFT( (opus_int32)W_tmp_QW[ i ], 18 - NLSF_W_Q ) );
            res_Q10[ i ] = (opus_int16)silk_RSHIFT( silk_SMULBB( res_Q15[ i ], W_tmp_Q9 ), 14 );
        }

        /* Modify input weights accordingly */
        for( i = 0; i < psNLSF_CB->order; i++ ) {
            W_adj_Q5[ i ] = silk_DIV32_16( silk_LSHIFT( (opus_int32)pW_QW[ i ], 5 ), W_tmp_QW[ i ] );
        }

        /* Unpack entropy table indices and predictor for current CB1 index */
        silk_NLSF_unpack( ec_ix, pred_Q8, psNLSF_CB, ind1 );

        /* Trellis quantizer */
        RD_Q25[ s ] = silk_NLSF_del_dec_quant( &tempIndices2[ s * MAX_LPC_ORDER ], res_Q10, W_adj_Q5, pred_Q8, ec_ix,
            psNLSF_CB->ec_Rates_Q5, psNLSF_CB->quantStepSize_Q16, psNLSF_CB->invQuantStepSize_Q6, NLSF_mu_Q20, psNLSF_CB->order );

        /* Add rate for first stage */
        iCDF_ptr = &psNLSF_CB->CB1_iCDF[ ( signalType >> 1 ) * psNLSF_CB->nVectors ];
        if( ind1 == 0 ) {
            prob_Q8 = 256 - iCDF_ptr[ ind1 ];
        } else {
            prob_Q8 = iCDF_ptr[ ind1 - 1 ] - iCDF_ptr[ ind1 ];
        }
        bits_q7 = ( 8 << 7 ) - silk_lin2log( prob_Q8 );
        RD_Q25[ s ] = silk_SMLABB( RD_Q25[ s ], bits_q7, silk_RSHIFT( NLSF_mu_Q20, 2 ) );
    }

    /* Find the lowest rate-distortion error */
    silk_insertion_sort_increasing( RD_Q25, &bestIndex, nSurvivors, 1 );

    NLSFIndices[ 0 ] = (opus_int8)tempIndices1[ bestIndex ];
    silk_memcpy( &NLSFIndices[ 1 ], &tempIndices2[ bestIndex * MAX_LPC_ORDER ], psNLSF_CB->order * sizeof( opus_int8 ) );

    /* Decode */
    silk_NLSF_decode( pNLSF_Q15, NLSFIndices, psNLSF_CB );

    RESTORE_STACK;

	/*NailTestPrintOutputShortArrayDeclaration("pNLSF_Q15", pNLSF_Q15, MAX_LPC_ORDER);
	NailTestPrintOutputIntDeclaration("returnVal", RD_Q25[0]);
	fprintf(stdout, "#endregion\n");
	fprintf(stdout, "int returnVal = NLSF.silk_NLSF_encode(in_NLSFIndices, in_pNLSF_Q15, in_psNLSF_CB, in_pW_QW, in_NLSF_mu_Q20, in_nSurvivors, in_signalType);\n");
	fprintf(stdout, "Assert.AreEqual(expected_returnVal, returnVal);\n");
	fprintf(stdout, "Helpers.AssertArrayDataEquals(expected_pNLSF_Q15, in_pNLSF_Q15);\n");
	NailTestPrintTestFooter();*/

    return RD_Q25[ 0 ];
}
