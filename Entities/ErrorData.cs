namespace Entities
{
    public class ErrorData
    {
        public enum ErrorCodes
        {
            NoError = 0,
            /* CDM */
            CDM_ERROR = 100,
            CDM_OPEN_ERROR = 101,
            CDM_DISPENSE_ERROR = 102,
            CDM_PRESENT_ERROR = 103,
            CDM_CUI_ERROR = 104,
            CDM_RETRACT_ERROR = 105,
            CDM_RETRACT_HAPPENED = 106,
            /*COIN*/
            COIN_ERROR = 200,
            COIN_OPEN_ERROR = 201,
            COIN_DISPENSE_ERROR = 202,
        }

        public ErrorCodes Code { get; set; }
        public string Message { get; set; }
    }
}
