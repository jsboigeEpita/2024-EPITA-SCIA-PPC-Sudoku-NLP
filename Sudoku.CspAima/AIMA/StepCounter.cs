using aima.core.search.csp;
using java.lang;

namespace Sudoku.CSPwithAIMA
{

    //Pour pouvoir faire de l'instrumentation au besoin
    public class StepCounter : CSPStateListener
    {

        public int AssignmentCount { get; set; }

        public int DomainCount { get; set; }

        public StepCounter()
        {
            this.AssignmentCount = 0;
            this.DomainCount = 0;
        }

        public virtual void stateChanged(Assignment assignment, aima.core.search.csp.CSP csp)
        {
            ++this.AssignmentCount;
        }

        public virtual void stateChanged(aima.core.search.csp.CSP csp)
        {
            ++this.DomainCount;
        }

        public virtual void reset()
        {
            this.AssignmentCount = 0;
            this.DomainCount = 0;
        }

        public virtual string getResults()
        {
            StringBuffer stringBuffer = new StringBuffer();
            stringBuffer.append(new StringBuilder().append("assignment changes: ").append(this.AssignmentCount).toString());
            if (this.DomainCount != 0)
                stringBuffer.append(new StringBuilder().append(", domain changes: ").append(this.DomainCount).toString());
            return stringBuffer.toString();
        }



        public override string ToString()
        {
            return getResults();
        }
    }
}