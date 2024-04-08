using aima.core.search.csp;
using java.lang;

namespace Sudoku.CSPwithAIMA
{

    // Class that implements CSPStateListener that ables to keep track of state changes of a solver
    public class StepCounter : CSPStateListener
    {

        public int AssignmentCount { get; set; }

        public int DomainCount { get; set; }

        public StepCounter()
        {
            this.AssignmentCount = 0;
            this.DomainCount = 0;
        }

        public void stateChanged(Assignment assignment, aima.core.search.csp.CSP csp)
        {
            ++this.AssignmentCount;
        }

        public void stateChanged(aima.core.search.csp.CSP csp)
        {
            ++this.DomainCount;
        }

        public void reset()
        {
            this.AssignmentCount = 0;
            this.DomainCount = 0;
        }

        public string getResults()
        {
            StringBuffer stringBuffer = new StringBuffer();
            stringBuffer.append(new StringBuilder().append("assignment changes: ").append(this.AssignmentCount).toString());
            stringBuffer.append(new StringBuilder().append(", domain changes: ").append(this.DomainCount).toString());
            return stringBuffer.toString();
        }

        public override string ToString()
        {
            return getResults();
        }
    }
}