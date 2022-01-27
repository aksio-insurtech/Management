// Copyright (c) Aksio Insurtech. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

import { useState } from 'react';
import { Nav, INavLinkGroup, INavLink, INavStyles, DefaultButton } from '@fluentui/react';
import { useNavigate } from 'react-router-dom';

import { default as styles } from './Applications.module.scss';
import { IModalProps, ModalButtons, ModalResult, useModal } from '@aksio/cratis-fluentui';
import { CreateApplicationDialog } from './CreateApplicationDialog';
import { CreateApplication } from 'API/Applications/CreateApplication';
import { Guid } from '@aksio/cratis-fundamentals';
import { AllApplications } from 'API/Applications/AllApplications';


const navStyles: Partial<INavStyles> = {
    root: {
        width: 200
    },
    link: {
        whiteSpace: 'normal',
        lineHeight: 'inherit',
    },
};

export const Applications = () => {
    const [selectedNav, setSelectedNav] = useState('');
    const [showCreateApplicationDialog] = useModal(
        "Create application",
        ModalButtons.OkCancel,
        CreateApplicationDialog,
        async (result, output) => {
            if (result == ModalResult.Success && output) {
                const command = new CreateApplication();
                command.applicationId = Guid.create().toString();
                command.name = output.name;
                await command.execute();
            }
        }
    );
    const [applications] = AllApplications.use();

    const groups: INavLinkGroup[] = [
        {
            links: applications.data.map(application => {
                return {
                    name: application.name,
                    url: '',
                    route: ''
                };
            })
        }];

    const history = useNavigate();

    const navItemClicked = (ev?: React.MouseEvent<HTMLElement>, item?: INavLink) => {
    };
    return (

        <div className={styles.applicationsContainer}>
            <div className={styles.applicationsNavigation}>
                <div className={styles.actions}>
                    <DefaultButton
                        text="Create"
                        style={{ width: 135 }}
                        iconProps={{ iconName: "Add" }}
                        onClick={showCreateApplicationDialog} />
                </div>
                <Nav
                    groups={groups}
                    styles={navStyles}
                    onLinkClick={navItemClicked}
                    selectedKey={selectedNav} />
            </div>
            <div style={{ width: '100%' }}>

            </div>
        </div>
    );
};
