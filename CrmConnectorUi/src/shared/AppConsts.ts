import { ContactModeInputEnum } from "./service-proxies/service-proxies";

export class AppConsts {

    static remoteServiceBaseUrl: string;
    static remoteServiceBaseUrlFormat: string;
    static appBaseUrl: string;
    static appBaseHref: string; // returns angular's base-href parameter value if used during the publish
    static appBaseUrlFormat: string;

    static title: string;

    static tempAgentName: string;

    static readonly webex = {
        agentNameKey: 'agent_name'
    };

    static readonly otpLength = 6;

    static ciscoParameters = {
        caseId: '', // required
        iPCCCallExtensionID: '', //required
        customerId: '', //guid
        customerIsAnonymous: false, //if customer is anonymous
        customerIsAuthenticated: false, //is otp validation is done
        systemAuthenticatedNAReason: '', //reason for why authentication is NA
        agentUsername: '', // required. Example: SVCSUAT\\SVCSTstCSOfficer
        agentName: '', // required. Example: John Doe
        contactMode: ContactModeInputEnum.Inbound, // required. Example: John Doe
        caseTitle: '', // required. Example: John Doe
        apiToken: '', // required for making api calls successfully
    }

    static ciscoParameters2 = {
        caseId: 'C-2022-07-07-0002', // required
        iPCCCallExtensionID: '7AA2354', //required
        customerId: '89c69f60-0116-ec11-878b-005056937f5c', //guid
        customerIsAnonymous: false, //if customer is anonymous
        customerIsAuthenticated: false, //is otp validation is done
        agentUsername: 'SVCSUAT\\SVCSTstCSOfficer', // required. Example: SVCSUAT\\SVCSTstCSOfficer
        agentName: 'Agent Test', // required. Example: John Doe
        contactMode: ContactModeInputEnum.Inbound, // required. Example: John Doe
        caseTitle: 'Case Title Test', // required. Example: John Doe
    }

    static parametersAreOk = false;

    /// <summary>
    /// Gets current version of the application.
    /// It's also shown in the web page.
    /// </summary>
    static readonly WebAppGuiVersion = '1.0.0';
    static AvailableLatestVersion = '1.0.0'; //from appconfig.json

    static InstrumentationKey = "b5869971-6179-42e9-9d62-c9a248aa37f3";

    static readonly categorySelect = {
        Category1: "Category1",
        Category2: "Category2",
        Category3: "Category3"
    };

    static isBrowser: boolean = true;
}
