namespace AntiBonto
{
    class Edge
    {
        private Person person2;
        private bool dislike;
        private string reason;
        private Person person1;

        public Person Person1
        {
            get
            {
                return person1;
            }

            set
            {
                person1 = value;
            }
        }

        public Person Person2
        {
            get
            {
                return person2;
            }

            set
            {
                person2 = value;
            }
        }

        public bool Dislike
        {
            get
            {
                return dislike;
            }

            set
            {
                dislike = value;
            }
        }

        public string Reason
        {
            get
            {
                return reason;
            }

            set
            {
                reason = value;
            }
        }
    }
}
