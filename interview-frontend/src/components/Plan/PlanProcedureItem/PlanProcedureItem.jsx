import React, { useEffect, useState } from "react";
import ReactSelect from "react-select";
import { addProcedureAssignment, getProcedureAssignments } from "../../../api/api";

const PlanProcedureItem = ({planId, procedure, users }) => {
    const [selectedUsers, setSelectedUsers] = useState(null);
    const { procedureId, procedureTitle } = procedure;

    useEffect(() => {
        (async () => {
            let userResponse = await getProcedureAssignments(procedureId);
            let selectedUser =  users.filter(u => userResponse.map(responseUser => responseUser.userId).includes(u.value));
            setSelectedUsers(selectedUser);
        })();
    }, [procedureId])

    const handleAssignUserToProcedure = async (selectedOptions) => {
          const selectedUserIds = selectedOptions.map((option) => option.value);
          const hasUser = selectedUsers.some((u) => selectedUserIds.includes(u.userId));
          if (hasUser) {
              console.log("User is already assigned to this procedure");
              return;
          }
          setSelectedUsers(selectedOptions);
          await addProcedureAssignment(planId, procedureId, selectedUserIds);
  
    };

    return (
        <div className="py-2">
            <div>
                {procedureTitle}
            </div>

            <ReactSelect
                className="mt-2"
                placeholder="Select User to Assign"
                isMulti={true}
                options={users}
                value={selectedUsers}
                onChange={(selectedOptions) => handleAssignUserToProcedure(selectedOptions)}
            />
        </div>
    );
};

export default PlanProcedureItem;
